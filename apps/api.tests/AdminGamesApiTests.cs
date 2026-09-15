using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminGamesApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public AdminGamesApiTests(ApiFactory factory)
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
    public async Task GetAdminGames_ReturnsTrackedGames_ForAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/games");
        var games = await response.Content.ReadFromJsonAsync<List<AdminGameDto>>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        games.ShouldNotBeNull();
        games.Count.ShouldBeGreaterThan(0);
        games.ShouldAllBe(g => g.SourceCount >= 0);
        games.ShouldAllBe(g => g.LeaderboardSyncStatus.Tier is "Hot" or "Cold");
    }

    [Fact]
    public async Task GetAdminGames_ReturnsHotLeaderboardSyncStatus_WhenGroupPlayedRecently()
    {
        // Arrange
        const int raGameId = 38130;
        var recentPlay = DateTimeOffset.UtcNow.AddHours(-1);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            game.LeaderboardScoresSyncedAt = DateTimeOffset.UtcNow.AddDays(-2);
            db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
            {
                MemberId = shrimp.Id,
                RaGameId = raGameId,
                Title = "Tracked",
                ConsoleId = 1,
                ConsoleName = "Genesis",
                LastPlayedAt = recentPlay,
                NumAchieved = 1,
                NumPossibleAchievements = 5,
                SyncedAt = recentPlay
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var games = await client.GetFromJsonAsync<List<AdminGameDto>>("/api/admin/games", JsonOptions);

        // Assert
        games.ShouldNotBeNull();
        var game = games!.Single(g => g.RaGameId == raGameId);
        game.LeaderboardSyncStatus.Tier.ShouldBe("Hot");
        game.LeaderboardSyncStatus.LeaderboardSyncIsDue.ShouldBeTrue();
    }

    [Fact]
    public async Task PatchAdminGameLeaderboardSync_ForcesColdSchedule()
    {
        // Arrange
        const int raGameId = 38130;
        var recentPlay = DateTimeOffset.UtcNow.AddHours(-1);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            game.LeaderboardScoresSyncedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
            db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
            {
                MemberId = shrimp.Id,
                RaGameId = raGameId,
                Title = "Tracked",
                ConsoleId = 1,
                ConsoleName = "Genesis",
                LastPlayedAt = recentPlay,
                NumAchieved = 1,
                NumPossibleAchievements = 5,
                SyncedAt = recentPlay
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PatchAsJsonAsync(
            $"/api/admin/games/{raGameId}/leaderboard-sync",
            new PatchAdminGameLeaderboardSyncRequest(true));
        var updated = await response.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull();
        updated!.ForceColdLeaderboardSync.ShouldBeTrue();
        updated.LeaderboardSyncStatus.Tier.ShouldBe("Cold");
        updated.LeaderboardSyncStatus.LeaderboardSyncForcedCold.ShouldBeTrue();
        updated.LeaderboardSyncStatus.LeaderboardSyncIsDue.ShouldBeFalse();
    }

    [Fact]
    public async Task GetAdminGames_ReturnsForbidden_ForNonAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAdminGame_RefreshAndDelete_WorkForAdmin()
    {
        // Arrange
        const int raGameId = 77777;
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
        SetupRaMocks(raGameId, includeMemberScore: false);
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act — add
        var addResponse = await admin.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));
        var created = await addResponse.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert — add
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        created.ShouldNotBeNull();
        created!.RaGameId.ShouldBe(raGameId);
        created.SourceCount.ShouldBe(0);

        // Act — refresh
        var refreshResponse = await admin.PostAsync($"/api/admin/games/{raGameId}/refresh", null);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert — refresh
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        refreshed.ShouldNotBeNull();
        refreshed!.MetadataSyncedAt.ShouldNotBeNull();

        // Act — delete
        var deleteResponse = await admin.DeleteAsync($"/api/admin/games/{raGameId}");

        // Assert — delete
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Games.AnyAsync(g => g.RaGameId == raGameId)).ShouldBeFalse();
    }

    private void SetupRaMocks(int raGameId, bool includeMemberScore)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto
            {
                Title = "Admin Test Game",
                ConsoleId = 3,
                ConsoleName = "SNES",
                ImageBoxArt = "/Images/box-admin.png"
            }));

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
            [
                new RaGameLeaderboardDto
                {
                    Id = raGameId * 10L,
                    Title = "High Score",
                    Format = "VALUE",
                    RankAsc = false
                }
            ]));

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(raGameId, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>([]));

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>(
            [
                new RaConsoleIdDto { Id = 3, Name = "SNES", IconUrl = "https://example.com/snes.png" }
            ]));
    }
}
