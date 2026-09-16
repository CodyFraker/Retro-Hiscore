using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class SyncHealthApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public SyncHealthApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSyncHealth_ReturnsHealthyByDefault()
    {
        // Arrange
        var finishedAt = DateTimeOffset.UtcNow;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Scheduled,
                Status = SyncRunStatus.Succeeded,
                StartedAt = finishedAt.AddMinutes(-5),
                FinishedAt = finishedAt
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var health = await client.GetFromJsonAsync<SyncHealthResponse>("/api/sync/health");

        // Assert
        health.ShouldNotBeNull();
        health.GroupStatus.ShouldBe(SyncGroupHealthEvaluator.StatusHealthy);
        health.MemberIssues.ShouldBeEmpty();
    }
}
