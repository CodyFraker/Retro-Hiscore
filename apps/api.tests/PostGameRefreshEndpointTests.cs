using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class PostGameRefreshEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public PostGameRefreshEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient
            .GetGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>([]));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostGameRefresh_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/games/38130/refresh", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostGameRefresh_WithoutApiKey_ReturnsForbidden()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", null);
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PostAsync("/api/games/38130/refresh", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostGameRefresh_WithApiKey_ReturnsAccepted()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PostAsync("/api/games/38130/refresh", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = await db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores && r.MemberId != null)
            .OrderByDescending(r => r.StartedAt)
            .FirstAsync();
        run.GameId.ShouldNotBeNull();
        run.MemberId.ShouldNotBeNull();
    }

    [Fact]
    public async Task PostGameRefresh_Returns429_WhenCooldownActive()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow,
                GameId = game.Id,
                MemberId = member.Id
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PostAsync("/api/games/38130/refresh", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
