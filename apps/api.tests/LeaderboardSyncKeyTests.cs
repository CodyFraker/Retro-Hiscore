using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class LeaderboardSyncKeyTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public LeaderboardSyncKeyTests(ApiFactory factory)
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
    public async Task Sync_SkipsMemberWithoutApiKey_ButSyncsOthers()
    {
        // Arrange
        await SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        await SetMemberApiKeyAsync("beefboybilly", null);
        await SetMemberApiKeyAsync("xXScubXx", null);

        SetupCatalogAndShrimpScores();

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ILeaderboardSyncService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();

        // Act
        var run = await sync.SyncGameWithRunAsync(game, SyncTrigger.Manual);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        await _factory.RaApiClient.Received(1).GetUserGameLeaderboardsAsync(
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_UsesMemberApiKey_ForUserGameLeaderboards()
    {
        // Arrange
        await SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        string? capturedKey = null;
        SetupCatalogAndShrimpScores();
        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedKey = call.ArgAt<string?>(2);
                return Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>(
                [
                    new RaUserGameLeaderboardDto
                    {
                        Id = 381300,
                        Title = "Main board",
                        RankAsc = false,
                        Format = "VALUE",
                        UserEntry = new RaUserEntryDto
                        {
                            User = "ShrimpPoboy",
                            Score = 100,
                            FormattedScore = "100",
                            Rank = 1
                        }
                    }
                ]);
            });

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ILeaderboardSyncService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();

        // Act
        await sync.SyncGameWithRunAsync(game, SyncTrigger.Manual);

        // Assert
        capturedKey.ShouldBe("shrimp-key");
    }

    [Fact]
    public async Task Sync_FailoversToNextKey_WhenFirstKeyRateLimited()
    {
        // Arrange
        await SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        await SetMemberApiKeyAsync("beefboybilly", "billy-key");

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var key = call.ArgAt<string?>(1);
                if (key == "test-key")
                {
                    throw new RaApiRateLimitedException("limited");
                }

                return Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
                [
                    new RaGameLeaderboardDto
                    {
                        Id = 381300,
                        Title = "Main board",
                        Format = "VALUE",
                        RankAsc = false
                    }
                ]);
            });

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(Arg.Any<int>(), "ShrimpPoboy", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>(
            [
                new RaUserGameLeaderboardDto
                {
                    Id = 381300,
                    Title = "Main board",
                    RankAsc = false,
                    Format = "VALUE",
                    UserEntry = new RaUserEntryDto
                    {
                        User = "ShrimpPoboy",
                        Score = 200,
                        FormattedScore = "200",
                        Rank = 2
                    }
                }
            ]));

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ILeaderboardSyncService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();

        // Act
        var run = await sync.SyncGameWithRunAsync(game, SyncTrigger.Manual);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        await _factory.RaApiClient.Received().GetGameLeaderboardsAsync(
            Arg.Any<int>(),
            "test-key",
            Arg.Any<CancellationToken>());
        await _factory.RaApiClient.Received().GetGameLeaderboardsAsync(
            Arg.Any<int>(),
            Arg.Is<string?>(key => key != null && key != "test-key"),
            Arg.Any<CancellationToken>());
    }

    private void SetupCatalogAndShrimpScores()
    {
        _factory.RaApiClient
            .GetGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var gameId = call.ArgAt<int>(0);
                return Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
                [
                    new RaGameLeaderboardDto
                    {
                        Id = gameId * 10L,
                        Title = "Main board",
                        Format = "VALUE",
                        RankAsc = false
                    }
                ]);
            });

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(Arg.Any<int>(), "ShrimpPoboy", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>(
            [
                new RaUserGameLeaderboardDto
                {
                    Id = 381300,
                    Title = "Main board",
                    RankAsc = false,
                    Format = "VALUE",
                    UserEntry = new RaUserEntryDto
                    {
                        User = "ShrimpPoboy",
                        Score = 100,
                        FormattedScore = "100",
                        Rank = 1
                    }
                }
            ]));
    }

    private async Task SetMemberApiKeyAsync(string raUsername, string? apiKey)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == raUsername);
        member.RaApiKey = apiKey;
        await db.SaveChangesAsync();
    }
}
