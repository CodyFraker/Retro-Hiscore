using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetGameTrackRequestQuotaEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetGameTrackRequestQuotaEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGameTrackRequestQuota_ReturnsRemainingAfterUsage()
    {
        // Arrange
        Guid memberId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            memberId = db.Members.Single(m => m.DiscordId == AuthTestHelper.SecondAllowedDiscordUserId).Id;
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < 2; i++)
            {
                db.GameTrackQueueRequests.Add(new GameTrackQueueRequest
                {
                    MemberId = memberId,
                    RaGameId = 90000 + i,
                    CreatedAt = now.AddMinutes(-i)
                });
            }

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var quota = await client.GetFromJsonAsync<GameTrackRequestQuotaDto>("/api/games/track-requests/quota");

        // Assert
        quota.ShouldNotBeNull();
        quota!.Limit.ShouldBe(5);
        quota.Used.ShouldBe(2);
        quota.Remaining.ShouldBe(3);
        quota.NextSlotAt.ShouldBeNull();
    }
}
