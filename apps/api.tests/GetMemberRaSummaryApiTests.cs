using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetMemberRaSummaryApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetMemberRaSummaryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMemberRaSummary_Returns404_WhenMemberMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/unknown-user/ra-summary");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberRaSummary_ReturnsUnavailable_WhenRaReturnsNull()
    {
        // Arrange
        _factory.RaApiClient
            .GetUserSummaryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RaUserSummaryDto?>(null));

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberRaSummaryResponse>("/api/members/ShrimpPoboy/ra-summary");

        // Assert
        body.ShouldNotBeNull();
        body.Available.ShouldBeFalse();
        body.Summary.ShouldBeNull();
        body.UnavailableReason.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetMemberRaSummary_ReturnsMappedSummary_WhenRaReturnsData()
    {
        // Arrange
        _factory.RaApiClient
            .GetUserSummaryAsync(
                "ShrimpPoboy",
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RaUserSummaryDto?>(new RaUserSummaryDto
            {
                User = "ShrimpPoboy",
                Ulid = "test-ulid",
                Rank = 100,
                TotalRanked = 1000,
                TotalPoints = 500,
                TotalTruePoints = 1200,
                MemberSince = "2020-01-15 12:00:00",
                Status = JsonSerializer.SerializeToElement("Online"),
                UserPic = "/UserPic/ShrimpPoboy.png",
                LastGame = new RaUserSummaryGameDto
                {
                    Id = 38130,
                    Title = "Test Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis",
                    ImageBoxArt = "/Images/box.png"
                },
                RecentAchievements = new Dictionary<string, Dictionary<string, RaUserSummaryRecentAchievementDto>>
                {
                    ["38130"] = new Dictionary<string, RaUserSummaryRecentAchievementDto>
                    {
                        ["1"] = new RaUserSummaryRecentAchievementDto
                        {
                            Id = 1,
                            GameId = 38130,
                            GameTitle = "Test Game",
                            Title = "First Steps",
                            Points = 5,
                            BadgeName = "12345",
                            DateAwarded = "2024-06-01 10:00:00",
                            HardcoreAchieved = 1
                        },
                        ["2"] = new RaUserSummaryRecentAchievementDto
                        {
                            Id = 2,
                            GameId = 38130,
                            GameTitle = "Test Game",
                            Title = "Older Unlock",
                            Points = 10,
                            BadgeName = "99999",
                            DateAwarded = "2024-05-01 10:00:00",
                            HardcoreAchieved = 0
                        }
                    }
                },
                Awarded = new Dictionary<string, RaUserSummaryAwardedDto>
                {
                    ["38130"] = new RaUserSummaryAwardedDto
                    {
                        NumAchieved = 2,
                        NumPossibleAchievements = 50,
                        ScoreAchieved = 15,
                        PossibleScore = 500
                    }
                }
            }));

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberRaSummaryResponse>("/api/members/ShrimpPoboy/ra-summary");

        // Assert
        body.ShouldNotBeNull();
        body.Available.ShouldBeTrue();
        body.Summary.ShouldNotBeNull();
        body.Summary!.Rank.ShouldBe(100);
        body.Summary.TotalRanked.ShouldBe(1000);
        body.Summary.UserPicUrl.ShouldBe("https://retroachievements.org/UserPic/ShrimpPoboy.png");
        body.Summary.Presence.ShouldNotBeNull();
        body.Summary.Presence!.RaGameId.ShouldBe(38130);
        body.Summary.Presence.IsTracked.ShouldBeTrue();
        body.Summary.Presence.Progress!.AchievementsEarned.ShouldBe(2);
        body.Summary.RecentAchievements.Count.ShouldBe(2);
        body.Summary.RecentAchievements[0].Title.ShouldBe("First Steps");
        body.Summary.RecentAchievements[0].BadgeUrl.ShouldBe("https://media.retroachievements.org/Badge/12345.png");
        body.Summary.RecentAchievements[0].HardcoreAchieved.ShouldBeTrue();
    }

    [Fact]
    public async Task GetMemberRaSummary_IncludesDbUnlockedAchievements_OnPresence()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 42,
                RaGameId = 38130,
                Title = "DB Badge",
                Points = 3,
                TrueRatio = 3,
                DisplayOrder = 0,
                BadgeData = ApiFactory.SamplePngBytes,
                BadgeContentType = "image/png"
            });
            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = 42,
                DateEarned = DateTimeOffset.Parse("2024-06-01 10:00:00"),
                DateEarnedHardcore = DateTimeOffset.Parse("2024-06-01 10:00:00"),
                FirstDetectedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        _factory.RaApiClient
            .GetUserSummaryAsync(
                "ShrimpPoboy",
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RaUserSummaryDto?>(new RaUserSummaryDto
            {
                User = "ShrimpPoboy",
                LastGame = new RaUserSummaryGameDto
                {
                    Id = 38130,
                    Title = "Test Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis"
                }
            }));

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberRaSummaryResponse>("/api/members/ShrimpPoboy/ra-summary");

        // Assert
        body.ShouldNotBeNull();
        body.Summary!.Presence!.UnlockedAchievements.Count.ShouldBe(1);
        body.Summary.Presence.UnlockedAchievements[0].Title.ShouldBe("DB Badge");
        body.Summary.Presence.UnlockedAchievements[0].BadgeUrl.ShouldBe(
            ConsoleIconSyncService.ToDataUrl(ApiFactory.SamplePngBytes, "image/png"));
        body.Summary.Presence.UnlockedAchievements[0].HardcoreAchieved.ShouldBeTrue();
    }
}
