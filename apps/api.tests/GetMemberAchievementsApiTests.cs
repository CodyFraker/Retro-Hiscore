using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetMemberAchievementsApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetMemberAchievementsApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMemberAchievements_TrackedOnly_FiltersToTrackedGames()
    {
        // Arrange
        const int trackedGameId = 38130;
        const int otherGameId = 99999;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.RaAchievements.AddRange(
                new RaAchievement
                {
                    RaAchievementId = 8101,
                    RaGameId = trackedGameId,
                    Title = "Tracked",
                    Points = 1,
                    TrueRatio = 1,
                    DisplayOrder = 1
                },
                new RaAchievement
                {
                    RaAchievementId = 8102,
                    RaGameId = otherGameId,
                    Title = "Other",
                    Points = 1,
                    TrueRatio = 1,
                    DisplayOrder = 1
                });
            db.MemberRaAchievements.AddRange(
                new MemberRaAchievement
                {
                    MemberId = member.Id,
                    RaAchievementId = 8101,
                    DateEarned = DateTimeOffset.UtcNow
                },
                new MemberRaAchievement
                {
                    MemberId = member.Id,
                    RaAchievementId = 8102,
                    DateEarned = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/ShrimpPoboy/achievements?trackedOnly=true");
        var body = await response.Content.ReadFromJsonAsync<MemberAchievementsListResponse>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body!.Items.Count.ShouldBe(1);
        body.Items[0].Title.ShouldBe("Tracked");
        body.Items[0].IsTracked.ShouldBeTrue();
    }

    [Fact]
    public async Task GetMemberAchievements_ReturnsAchievementsProgressSyncedAt()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.MemberAchievements,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = syncedAt,
                FinishedAt = syncedAt,
                MemberId = member.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberAchievementsListResponse>(
            "/api/members/ShrimpPoboy/achievements");

        // Assert
        body.ShouldNotBeNull();
        body!.AchievementsProgressSyncedAt.ShouldBe(syncedAt);
    }
}
