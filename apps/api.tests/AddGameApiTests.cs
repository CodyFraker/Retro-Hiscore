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
public class AddGameApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public AddGameApiTests(ApiFactory factory)
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
    public async Task AddGame_CreatesGame_SyncsMetadataCatalogAndMemberScores()
    {
        // Arrange
        const int raGameId = 99999;
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
        SetupRaMocks(raGameId, includeMemberScore: true);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));
        var created = await response.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);
        var games = await client.GetFromJsonAsync<List<GameDto>>("/api/games", JsonOptions);
        var detail = await client.GetFromJsonAsync<GameLeaderboardsResponse>($"/api/games/{raGameId}/leaderboards", JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        created.ShouldNotBeNull();
        created.RaGameId.ShouldBe(raGameId);
        created.Title.ShouldBe("New Adventure");
        created.ConsoleName.ShouldBe("SNES");
        created.ConsoleIconUrl.ShouldBe(
            ConsoleIconSyncService.ToDataUrl(ApiFactory.SamplePngBytes, "image/png"));
        created.ImageBoxArtUrl.ShouldBe("https://media.retroachievements.org/Images/box-new.png");
        created.LeaderboardCount.ShouldBe(1);

        games.ShouldNotBeNull();
        games.ShouldContain(g => g.RaGameId == raGameId && g.Title == "New Adventure");

        detail.ShouldNotBeNull();
        detail.Leaderboards.Count.ShouldBe(1);
        detail.Members.Count.ShouldBe(1);
        detail.Members[0].RaUsername.ShouldBe("ShrimpPoboy");
        var shrimp = detail.Leaderboards[0].Standings.Single(s => s.RaUsername == "ShrimpPoboy");
        shrimp.Score.ShouldBe(12345);
        shrimp.FriendRank.ShouldBe(1);
    }

    [Fact]
    public async Task AddGame_Returns409_WhenGameAlreadyTracked()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(38130));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        await _factory.RaApiClient.DidNotReceiveWithAnyArgs().GetGameAsync(default, default, default);
    }

    [Fact]
    public async Task AddGame_Returns404_WhenRaGameMissing()
    {
        // Arrange
        const int raGameId = 40404;
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto { Title = "" }));
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Games.AnyAsync(g => g.RaGameId == raGameId)).ShouldBeFalse();
    }

    [Fact]
    public async Task AddGame_CreatesGame_WhenNoMembersHaveScores()
    {
        // Arrange
        const int raGameId = 88888;
        SetupRaMocks(raGameId, includeMemberScore: false);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));
        var created = await response.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);
        var detail = await client.GetFromJsonAsync<GameLeaderboardsResponse>($"/api/games/{raGameId}/leaderboards", JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        created.ShouldNotBeNull();
        created.LeaderboardCount.ShouldBe(1);
        detail.ShouldNotBeNull();
        detail.Leaderboards.Count.ShouldBe(1);
        detail.Members.Count.ShouldBe(0);
        detail.Leaderboards[0].Standings.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddGame_CompletesPendingTrackQueueForSameRaGameId()
    {
        // Arrange
        const int raGameId = 99998;
        SetupRaMocks(raGameId, includeMemberScore: false);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameTrackQueues.Add(new GameTrackQueue
            {
                RaGameId = raGameId,
                Status = GameTrackQueueStatus.Pending,
                Title = "Pending",
                EnqueuedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var queue = await verifyDb.GameTrackQueues.SingleAsync(q => q.RaGameId == raGameId);
        queue.Status.ShouldBe(GameTrackQueueStatus.Completed);
    }

    private void SetupRaMocks(int raGameId, bool includeMemberScore)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto
            {
                Title = "New Adventure",
                ConsoleId = 3,
                ConsoleName = "SNES",
                ImageIcon = "/Images/icon-new.png",
                ImageBoxArt = "/Images/box-new.png",
                Publisher = "Nintendo",
                Developer = "Nintendo",
                Genre = "Action",
                Released = "1994-01-01 00:00:00"
            }));

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
            [
                new RaGameLeaderboardDto
                {
                    Id = raGameId * 10L,
                    Title = "High Score",
                    Description = "Best score",
                    Format = "VALUE",
                    RankAsc = false
                }
            ]));

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(raGameId, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var user = call.ArgAt<string>(1);
                if (!includeMemberScore ||
                    !string.Equals(user, "ShrimpPoboy", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>([]);
                }

                return Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>(
                [
                    new RaUserGameLeaderboardDto
                    {
                        Id = raGameId * 10L,
                        Title = "High Score",
                        RankAsc = false,
                        Format = "VALUE",
                        UserEntry = new RaUserEntryDto
                        {
                            User = "ShrimpPoboy",
                            Ulid = "01ADDGAMEULID",
                            Score = 12345,
                            FormattedScore = "12,345",
                            Rank = 42,
                            DateUpdated = DateTimeOffset.UtcNow
                        }
                    }
                ]);
            });

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>(
            [
                new RaConsoleIdDto
                {
                    Id = 3,
                    Name = "SNES",
                    IconUrl = "https://static.retroachievements.org/assets/images/system/snes.png"
                }
            ]));
    }
}
