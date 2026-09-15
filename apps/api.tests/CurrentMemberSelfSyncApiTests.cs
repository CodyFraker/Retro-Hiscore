using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class CurrentMemberSelfSyncApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public CurrentMemberSelfSyncApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSyncStatus_ReturnsScopes()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var status = await client.GetFromJsonAsync<MemberSelfSyncStatusDto>(
            "/api/members/me/sync-status",
            JsonOptions);

        // Assert
        status.ShouldNotBeNull();
        status.Leaderboards.ShouldNotBeNull();
        status.Profile.ShouldNotBeNull();
        status.Achievements.ShouldNotBeNull();
    }

    [Fact]
    public async Task PostLeaderboards_WithoutApiKey_ReturnsForbidden()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", null);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/members/me/sync/leaderboards", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
    }

    [Fact]
    public async Task PostLeaderboards_QueuesJobsForTrackedGames()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/members/me/sync/leaderboards", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        var runs = db.SyncRuns
            .Where(r => r.MemberId == member.Id && r.Kind == SyncKind.LeaderboardScores)
            .ToList();
        runs.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task PostProfile_CreatesMemberScopedRuns()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/members/me/sync/profile", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        db.SyncRuns.Any(r =>
                r.MemberId == member.Id
                && r.Kind == SyncKind.MemberRank
                && r.Trigger == SyncTrigger.Manual)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task PostAchievements_Returns429_WhenCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.MemberAchievements,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow,
                MemberId = member.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/members/me/sync/achievements", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GetSyncStatus_ReturnsForbidden_WhenDiscordUserHasNoMemberRecord()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("000000000000000001");

        // Act
        var response = await client.GetAsync("/api/members/me/sync-status");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAchievements_CooldownIsScopedPerMember()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("beefboybilly", "billy-test-key");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.MemberAchievements,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow,
                MemberId = shrimp.Id
            });
            await db.SaveChangesAsync();
        }

        var shrimpClient = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var billyClient = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var shrimpResponse = await shrimpClient.PostAsync("/api/members/me/sync/achievements", null);
        var billyResponse = await billyClient.PostAsync("/api/members/me/sync/achievements", null);

        // Assert
        shrimpResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        billyResponse.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task PostLeaderboards_Returns429_WhenMemberCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow,
                MemberId = member.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/members/me/sync/leaderboards", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
