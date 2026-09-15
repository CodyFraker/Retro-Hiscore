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
public class GameMetadataApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public GameMetadataApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient.ClearReceivedCalls();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MetadataSync_PersistsTitlesAndImageUrls()
    {
        // Arrange
        _factory.RaApiClient
            .GetGameAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var gameId = call.ArgAt<int>(0);
                return Task.FromResult(new RaGameDto
                {
                    Title = $"Title {gameId}",
                    ConsoleId = 1,
                    ConsoleName = "Mega Drive",
                    ImageIcon = "/Images/icon.png",
                    ImageTitle = "/Images/title.png",
                    ImageIngame = "/Images/ingame.png",
                    ImageBoxArt = "/Images/box.png",
                    Publisher = "Sega",
                    Developer = "Sonic Team",
                    Genre = "Platformer",
                    Released = "1991-06-23 00:00:00"
                });
            });

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<IGameMetadataSyncService>();

        // Act
        var run = await sync.SyncAsync(SyncTrigger.Manual);
        var client = _factory.CreateAuthenticatedClient();
        var games = await client.GetFromJsonAsync<List<GameDto>>("/api/games", JsonOptions);
        var detail = await client.GetFromJsonAsync<GameLeaderboardsResponse>("/api/games/38130/leaderboards", JsonOptions);
        var status = await client.GetFromJsonAsync<SyncStatusDto>("/api/sync/metadata/status", JsonOptions);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        run.Kind.ShouldBe(SyncKind.GameMetadata);

        games.ShouldNotBeNull();
        var game = games.Single(g => g.RaGameId == 38130);
        game.Title.ShouldBe("Title 38130");
        game.ConsoleName.ShouldBe("Mega Drive");
        game.ImageBoxArtUrl.ShouldBe("https://media.retroachievements.org/Images/box.png");
        game.ImageIconUrl.ShouldBe("https://media.retroachievements.org/Images/icon.png");

        detail.ShouldNotBeNull();
        detail.Title.ShouldBe("Title 38130");
        detail.ConsoleName.ShouldBe("Mega Drive");
        detail.ImageBoxArtUrl.ShouldBe("https://media.retroachievements.org/Images/box.png");
        detail.Publisher.ShouldBe("Sega");
        detail.Developer.ShouldBe("Sonic Team");
        detail.Genre.ShouldBe("Platformer");
        detail.ReleasedAt.ShouldNotBeNull();
        detail.MetadataSyncedAt.ShouldNotBeNull();

        status.ShouldNotBeNull();
        status.Status.ShouldBe(nameof(SyncRunStatus.Succeeded));
    }

    [Fact]
    public async Task TriggerMetadataSync_Returns429_WhenCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.GameMetadata,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/metadata", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task TriggerMetadataSync_WithNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/metadata", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSyncStatus_IgnoresMetadataRuns()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Scheduled,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                FinishedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            });
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.GameMetadata,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Failed,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow,
                Error = "boom"
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var scoreStatus = await client.GetFromJsonAsync<SyncStatusDto>("/api/sync/status", JsonOptions);
        var metadataStatus = await client.GetFromJsonAsync<SyncStatusDto>("/api/sync/metadata/status", JsonOptions);

        // Assert
        scoreStatus.ShouldNotBeNull();
        scoreStatus.Status.ShouldBe(nameof(SyncRunStatus.Succeeded));
        scoreStatus.Trigger.ShouldBe(nameof(SyncTrigger.Scheduled));

        metadataStatus.ShouldNotBeNull();
        metadataStatus.Status.ShouldBe(nameof(SyncRunStatus.Failed));
        metadataStatus.Error.ShouldBe("boom");
    }
}
