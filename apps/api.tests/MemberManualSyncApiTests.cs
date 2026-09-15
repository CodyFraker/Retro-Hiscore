using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberManualSyncApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public MemberManualSyncApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient.ClearReceivedCalls();

        _factory.RaApiClient
            .GetUserRecentlyPlayedGamesAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserRecentlyPlayedGameDto>>([]));

        _factory.RaApiClient
            .GetUserSummaryAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RaUserSummaryDto?>(new RaUserSummaryDto { Rank = 1, TotalRanked = 100 }));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostMemberActivity_WithAdmin_CreatesManualRun()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/member-activity", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = db.SyncRuns
            .Where(r => r.Kind == SyncKind.MemberActivity && r.Trigger == SyncTrigger.Manual)
            .OrderByDescending(r => r.StartedAt)
            .First();
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
    }

    [Fact]
    public async Task GetMemberActivityStatus_ReturnsLatestRun()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        await client.PostAsync("/api/sync/member-activity", null);

        // Act
        var status = await client.GetFromJsonAsync<SyncStatusDto>("/api/sync/member-activity/status", JsonOptions);

        // Assert
        status.ShouldNotBeNull();
        status.Status.ShouldBe(nameof(SyncRunStatus.Succeeded));
        status.Trigger.ShouldBe(nameof(SyncTrigger.Manual));
    }

    [Fact]
    public async Task PostMemberActivity_Returns429_WhenCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.MemberActivity,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/member-activity", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task PostMemberRank_WithAdmin_CreatesManualRun()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/member-rank", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = db.SyncRuns
            .Where(r => r.Kind == SyncKind.MemberRank && r.Trigger == SyncTrigger.Manual)
            .OrderByDescending(r => r.StartedAt)
            .First();
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
    }

    [Fact]
    public async Task PostMemberRank_WithNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/member-rank", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
