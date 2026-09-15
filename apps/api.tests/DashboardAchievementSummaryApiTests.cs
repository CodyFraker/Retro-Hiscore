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
public class DashboardAchievementSummaryApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public DashboardAchievementSummaryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboardAchievementSummary_ReturnsCountsForRecentUnlocks()
    {
        // Arrange
        const int raGameId = 38130;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 8020,
                RaGameId = raGameId,
                Title = "Recent",
                Points = 1,
                TrueRatio = 1,
                DisplayOrder = 1
            });
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 8021,
                RaGameId = raGameId,
                Title = "Old",
                Points = 1,
                TrueRatio = 1,
                DisplayOrder = 2
            });
            db.MemberRaAchievements.AddRange(
                new MemberRaAchievement
                {
                    MemberId = member.Id,
                    RaAchievementId = 8020,
                    DateEarned = DateTimeOffset.UtcNow.AddDays(-2)
                },
                new MemberRaAchievement
                {
                    MemberId = member.Id,
                    RaAchievementId = 8021,
                    DateEarned = DateTimeOffset.UtcNow.AddDays(-20)
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/dashboard/achievement-summary");
        var body = await response.Content.ReadFromJsonAsync<DashboardAchievementSummaryResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body!.UnlocksLast7Days.ShouldBe(1);
        body.UnlocksLast30Days.ShouldBe(2);
        body.ActiveMembersLast7Days.ShouldBe(1);
        body.LastUnlockAt.ShouldNotBeNull();
        body.TopGameLast7Days.ShouldNotBeNull();
        body.TopGameLast7Days!.UnlockCount.ShouldBe(1);
    }
}
