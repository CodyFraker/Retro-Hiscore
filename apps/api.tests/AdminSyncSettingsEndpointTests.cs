using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminSyncSettingsEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public AdminSyncSettingsEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAdminSyncSettings_WithNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateToken(AuthTestHelper.SecondAllowedDiscordUserId));

        // Act
        var response = await client.GetAsync("/api/admin/sync-settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminSyncSettings_WithAdmin_ReturnsSeededDefaults()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/sync-settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AdminSyncSettingsDto>();
        body.ShouldNotBeNull();
        body.Leaderboard.HotIntervalMinutes.ShouldBe(15);
        body.Leaderboard.ColdIntervalMinutes.ShouldBe(1440);
        body.Leaderboard.HotActivityWindowHours.ShouldBe(168);
        body.RecurringJobs.Count.ShouldBe(5);
        body.RecurringJobs.Single(j => j.JobId == SyncRecurringJobIds.LeaderboardDispatch).IntervalMinutes.ShouldBe(5);
        body.RecurringJobs.Single(j => j.JobId == SyncRecurringJobIds.GameMetadata).IntervalDays.ShouldBe(7);
    }

    [Fact]
    public async Task PatchAdminSyncSettings_WithAdmin_PersistsChanges()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var patch = new PatchAdminSyncSettingsRequest(
            new PatchLeaderboardSyncSettingsRequest(20, 2000, 72),
            [
                new PatchRecurringJobSettingsRequest(SyncRecurringJobIds.MemberActivity, 10, null),
                new PatchRecurringJobSettingsRequest(SyncRecurringJobIds.GameMetadata, null, 3)
            ]);

        // Act
        var response = await client.PatchAsJsonAsync("/api/admin/sync-settings", patch);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AdminSyncSettingsDto>();
        body.ShouldNotBeNull();
        body.Leaderboard.HotIntervalMinutes.ShouldBe(20);
        body.RecurringJobs.Single(j => j.JobId == SyncRecurringJobIds.MemberActivity).IntervalMinutes.ShouldBe(10);
        body.RecurringJobs.Single(j => j.JobId == SyncRecurringJobIds.GameMetadata).IntervalDays.ShouldBe(3);
    }
}
