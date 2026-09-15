using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class DashboardAchievementApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public DashboardAchievementApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboardAchievementActivity_ReturnsTrackedGameUnlocks()
    {
        // Arrange
        const int raGameId = 38130;
        const int achievementId = 8001;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = achievementId,
                RaGameId = raGameId,
                Title = "Dash Ach",
                Points = 5,
                TrueRatio = 5,
                DisplayOrder = 1
            });
            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = achievementId,
                DateEarned = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/achievement-activity?limit=10");
        var body = await response.Content.ReadFromJsonAsync<DashboardAchievementActivityResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body!.Total.ShouldBeGreaterThan(0);
        body.Items.Count.ShouldBeGreaterThan(0);
        body.Items[0].Title.ShouldBe("Dash Ach");
    }

    [Fact]
    public async Task GetDashboardAchievementActivity_PaginatesAndFiltersByGame()
    {
        // Arrange
        const int gameA = 38130;
        const int gameB = 38131;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.Games.Add(new Game { RaGameId = gameB, Title = "Other Game" });
            db.RaAchievements.AddRange(
                new RaAchievement
                {
                    RaAchievementId = 8010,
                    RaGameId = gameA,
                    Title = "A1",
                    Points = 1,
                    TrueRatio = 1,
                    DisplayOrder = 1
                },
                new RaAchievement
                {
                    RaAchievementId = 8011,
                    RaGameId = gameA,
                    Title = "A2",
                    Points = 1,
                    TrueRatio = 1,
                    DisplayOrder = 2
                },
                new RaAchievement
                {
                    RaAchievementId = 8012,
                    RaGameId = gameB,
                    Title = "B1",
                    Points = 1,
                    TrueRatio = 1,
                    DisplayOrder = 1
                });
            var t0 = DateTimeOffset.UtcNow.AddHours(-2);
            var t1 = DateTimeOffset.UtcNow.AddHours(-1);
            var t2 = DateTimeOffset.UtcNow;
            db.MemberRaAchievements.AddRange(
                new MemberRaAchievement { MemberId = member.Id, RaAchievementId = 8010, DateEarned = t0 },
                new MemberRaAchievement { MemberId = member.Id, RaAchievementId = 8011, DateEarned = t1 },
                new MemberRaAchievement { MemberId = member.Id, RaAchievementId = 8012, DateEarned = t2 });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var page = await client.GetFromJsonAsync<DashboardAchievementActivityResponse>(
            "/api/dashboard/achievement-activity?limit=1&offset=1&raGameId=38130");

        // Assert
        page.ShouldNotBeNull();
        page!.Total.ShouldBe(2);
        page.Offset.ShouldBe(1);
        page.Limit.ShouldBe(1);
        page.Items.Count.ShouldBe(1);
        page.Items[0].Title.ShouldBe("A1");
    }
}
