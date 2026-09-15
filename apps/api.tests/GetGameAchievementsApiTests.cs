using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetGameAchievementsApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public GetGameAchievementsApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGameAchievements_Returns404_WhenGameNotTracked()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/games/99999/achievements");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGameAchievements_ReturnsCatalogAndUnlocks()
    {
        // Arrange
        const int raGameId = 38130;
        const int achievementId = 7001;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = achievementId,
                RaGameId = raGameId,
                Title = "Test Ach",
                Points = 10,
                TrueRatio = 20,
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
        var response = await client.GetAsync($"/api/games/{raGameId}/achievements");
        var body = await response.Content.ReadFromJsonAsync<GameAchievementsResponse>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body!.Achievements.Count.ShouldBe(1);
        body.Achievements[0].Title.ShouldBe("Test Ach");
        body.Unlocks.Count.ShouldBe(1);
        body.MemberSummaries.Any(s => s.AchievementsEarned == 1).ShouldBeTrue();
    }

    [Fact]
    public async Task GetGameAchievementDistribution_ReturnsUnavailable_WhenNotSynced()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/games/38130/achievement-distribution");
        var body = await response.Content.ReadFromJsonAsync<GameAchievementDistributionResponse>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body!.Available.ShouldBeFalse();
    }
}
