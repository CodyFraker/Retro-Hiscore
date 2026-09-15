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
public class GetMemberRaAchievementHistoryApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetMemberRaAchievementHistoryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMemberRaAchievementHistory_Returns404_WhenMemberMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/unknown-user/ra-achievement-history");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberRaAchievementHistory_ReturnsCumulativeSeries_WhenUnlocksExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");

        db.RaAchievements.AddRange(
            new RaAchievement
            {
                RaAchievementId = 1,
                RaGameId = 100,
                Title = "A",
                Points = 5,
                TrueRatio = 5,
                DisplayOrder = 1,
                BadgeData = ApiFactory.SamplePngBytes,
                BadgeContentType = "image/png"
            },
            new RaAchievement
            {
                RaAchievementId = 2,
                RaGameId = 100,
                Title = "B",
                Points = 10,
                TrueRatio = 15,
                DisplayOrder = 2,
                BadgeData = ApiFactory.SamplePngBytes,
                BadgeContentType = "image/png"
            });

        db.MemberRaAchievements.AddRange(
            new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = 1,
                DateEarned = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
                FirstDetectedAt = DateTimeOffset.UtcNow
            },
            new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = 2,
                DateEarned = new DateTimeOffset(2024, 2, 1, 12, 0, 0, TimeSpan.Zero),
                FirstDetectedAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberRaAchievementHistoryResponse>(
            "/api/members/ShrimpPoboy/ra-achievement-history");

        // Assert
        body.ShouldNotBeNull();
        body.Items.Count.ShouldBe(2);
        body.Items[0].PointsEarned.ShouldBe(5);
        body.Items[0].TruePointsEarned.ShouldBe(5);
        body.Items[0].CumulativeUnlocks.ShouldBe(1);
        body.Items[0].CumulativePoints.ShouldBe(5);
        body.Items[0].CumulativeTruePoints.ShouldBe(5);
        body.Items[1].PointsEarned.ShouldBe(10);
        body.Items[1].TruePointsEarned.ShouldBe(15);
        body.Items[1].CumulativeUnlocks.ShouldBe(2);
        body.Items[1].CumulativePoints.ShouldBe(15);
        body.Items[1].CumulativeTruePoints.ShouldBe(20);
    }
}
