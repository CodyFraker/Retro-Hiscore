using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class SyncDispatchApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public SyncDispatchApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostDispatch_WithAdmin_ReturnsAccepted()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/dispatch", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var body = await response.Content.ReadFromJsonAsync<SyncAcceptedResponse>(JsonOptions);
        body.ShouldNotBeNull();
        body.JobId.ShouldStartWith("enqueued-");
    }

    [Fact]
    public async Task PostDispatch_WithNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/dispatch", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostDispatch_Returns429_WhenLeaderboardCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync/dispatch", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
