using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberRaRankSnapshotSyncTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberRaRankSnapshotSyncTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient.ClearReceivedCalls();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Sync_RecordsMemberRaRankSnapshot_FromUserSummary()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        await _factory.SetMemberApiKeyAsync("beefboybilly", null);

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>([]));

        _factory.RaApiClient
            .GetUserSummaryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var target = call.ArgAt<string>(0);
                var isShrimp = target.Contains("Shrimp", StringComparison.OrdinalIgnoreCase);
                return Task.FromResult<RaUserSummaryDto?>(new RaUserSummaryDto
                {
                    User = isShrimp ? "ShrimpPoboy" : "beefboybilly",
                    Rank = isShrimp ? 117_215 : 50_000,
                    TotalRanked = 163_826,
                    TotalPoints = isShrimp ? 534 : 200,
                    TotalTruePoints = isShrimp ? 1_060 : 400,
                    TotalSoftcorePoints = isShrimp ? 14 : 0
                });
            });

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ILeaderboardSyncService>();

        // Act
        var run = await sync.SyncAsync(SyncTrigger.Manual);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var shrimpSnapshots = await db.MemberRaRankSnapshots
            .Where(s => s.Member.RaUsername == "ShrimpPoboy")
            .ToListAsync();
        shrimpSnapshots.Count.ShouldBe(1);
        shrimpSnapshots[0].Rank.ShouldBe(117_215);
        shrimpSnapshots[0].TotalRanked.ShouldBe(163_826);
        shrimpSnapshots[0].TotalPoints.ShouldBe(534);
        shrimpSnapshots[0].TotalTruePoints.ShouldBe(1_060);
        shrimpSnapshots[0].TotalSoftcorePoints.ShouldBe(14);

        var billySnapshots = await db.MemberRaRankSnapshots
            .Where(s => s.Member.RaUsername == "beefboybilly")
            .CountAsync();
        billySnapshots.ShouldBe(1);
    }
}
