using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class ConsoleIconSyncApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public ConsoleIconSyncApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient.ClearReceivedCalls();
        _factory.ConsoleIconDownloader.ClearReceivedCalls();

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>(
            [
                new RaConsoleIdDto
                {
                    Id = 1,
                    Name = "Mega Drive",
                    IconUrl = "https://static.retroachievements.org/assets/images/system/md.png"
                },
                new RaConsoleIdDto
                {
                    Id = 3,
                    Name = "SNES",
                    IconUrl = "https://static.retroachievements.org/assets/images/system/snes.png"
                }
            ]));

        _factory.ConsoleIconDownloader
            .DownloadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ConsoleIconSync_DownloadsOnlyNeededConsoles_AndExposesUrlOnGames()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var seeded = db.Games.Single(g => g.RaGameId == 38130);
            seeded.ConsoleId = 1;
            seeded.ConsoleName = "Mega Drive";
            await db.SaveChangesAsync();
        }

        using var syncScope = _factory.Services.CreateScope();
        var sync = syncScope.ServiceProvider.GetRequiredService<IConsoleIconSyncService>();

        // Act
        var run = await sync.SyncAsync(SyncTrigger.Manual);
        var client = _factory.CreateAuthenticatedClient();
        var games = await client.GetFromJsonAsync<List<GameDto>>("/api/games", JsonOptions);
        var detail = await client.GetFromJsonAsync<GameLeaderboardsResponse>("/api/games/38130/leaderboards", JsonOptions);
        var status = await client.GetFromJsonAsync<SyncStatusDto>("/api/sync/console-icons/status", JsonOptions);
        var iconResponse = await client.GetAsync("/system-icons/1.png");

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        run.Kind.ShouldBe(SyncKind.ConsoleIcons);

        await _factory.RaApiClient.Received(1).GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _factory.ConsoleIconDownloader.Received(1)
            .DownloadAsync("https://static.retroachievements.org/assets/images/system/md.png", Arg.Any<CancellationToken>());

        File.Exists(Path.Combine(_factory.SystemIconStoragePath, "1.png")).ShouldBeTrue();
        File.Exists(Path.Combine(_factory.SystemIconStoragePath, "3.png")).ShouldBeFalse();

        games.ShouldNotBeNull();
        var gameDto = games.Single(g => g.RaGameId == 38130);
        gameDto.ConsoleIconUrl.ShouldNotBeNull();
        gameDto.ConsoleIconUrl.ShouldEndWith("/system-icons/1.png");

        detail.ShouldNotBeNull();
        detail.ConsoleIconUrl.ShouldEndWith("/system-icons/1.png");

        status.ShouldNotBeNull();
        status.Status.ShouldBe(nameof(SyncRunStatus.Succeeded));

        iconResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConsoleIconSync_SkipsDownload_WhenIconAlreadyPresent()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = db.Games.Single(g => g.RaGameId == 38130);
            game.ConsoleId = 1;
            game.ConsoleName = "Mega Drive";
            await db.SaveChangesAsync();
        }

        using var syncScope = _factory.Services.CreateScope();
        var sync = syncScope.ServiceProvider.GetRequiredService<IConsoleIconSyncService>();
        await sync.SyncAsync(SyncTrigger.Manual);
        _factory.ConsoleIconDownloader.ClearReceivedCalls();
        _factory.RaApiClient.ClearReceivedCalls();

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>(
            [
                new RaConsoleIdDto
                {
                    Id = 1,
                    Name = "Mega Drive",
                    IconUrl = "https://static.retroachievements.org/assets/images/system/md.png"
                }
            ]));

        // Act
        var run = await sync.SyncAsync(SyncTrigger.Manual, force: false);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        await _factory.ConsoleIconDownloader.DidNotReceiveWithAnyArgs()
            .DownloadAsync(default!, default);
    }

    [Fact]
    public async Task ConsoleIconSync_Force_RedownloadsIcons()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = db.Games.Single(g => g.RaGameId == 38130);
            game.ConsoleId = 1;
            game.ConsoleName = "Mega Drive";
            await db.SaveChangesAsync();
        }

        using var syncScope = _factory.Services.CreateScope();
        var sync = syncScope.ServiceProvider.GetRequiredService<IConsoleIconSyncService>();
        await sync.SyncAsync(SyncTrigger.Scheduled);
        _factory.ConsoleIconDownloader.ClearReceivedCalls();

        // Act
        var client = _factory.CreateAuthenticatedClient();
        var response = await client.PostAsync("/api/sync/console-icons?force=true", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        await _factory.ConsoleIconDownloader.Received(1)
            .DownloadAsync("https://static.retroachievements.org/assets/images/system/md.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerConsoleIconSync_Returns429_WhenCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.ConsoleIcons,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PostAsync("/api/sync/console-icons", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
