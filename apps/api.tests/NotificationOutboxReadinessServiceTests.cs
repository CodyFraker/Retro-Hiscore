using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Notifications;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class NotificationOutboxReadinessServiceTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public NotificationOutboxReadinessServiceTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MarkNotificationsReadyForSyncRunAsync_SetsReadyAt_WhenSyncSucceeded()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var finishedAt = DateTimeOffset.UtcNow;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Id = runId,
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = finishedAt.AddMinutes(-1),
                FinishedAt = finishedAt
            });
            db.NotificationOutbox.Add(new NotificationOutbox
            {
                EventKind = DiscordNotificationEventKind.GameTracked,
                PayloadJson = """{"raGameId":42}""",
                OccurredAt = finishedAt.AddMinutes(-1),
                SourceSyncRunId = runId,
                ReadyAt = null
            });
            await db.SaveChangesAsync();
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var readiness = scope.ServiceProvider.GetRequiredService<INotificationOutboxReadinessService>();

            // Act
            await readiness.MarkNotificationsReadyForSyncRunAsync(runId);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var outbox = await db.NotificationOutbox.SingleAsync();
            outbox.ReadyAt.ShouldNotBeNull();
            outbox.ReadyAt!.Value.ShouldBe(finishedAt, TimeSpan.FromSeconds(1));
        }
    }
}
