using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetMemberRaRankHistoryApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetMemberRaRankHistoryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMemberRaRankHistory_Returns404_WhenMemberMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/unknown-user/ra-rank-history");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberRaRankHistory_ReturnsSnapshotsOrderedBySyncedAt()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        var earlier = DateTimeOffset.UtcNow.AddDays(-2);
        var later = DateTimeOffset.UtcNow.AddDays(-1);
        db.MemberRaRankSnapshots.AddRange(
            new MemberRaRankSnapshot
            {
                MemberId = member.Id,
                Rank = 120_000,
                TotalPoints = 540,
                TotalTruePoints = 1_070,
                SyncedAt = later
            },
            new MemberRaRankSnapshot
            {
                MemberId = member.Id,
                Rank = 125_000,
                TotalPoints = 500,
                TotalTruePoints = 1_000,
                SyncedAt = earlier
            });
        await db.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var body = await client.GetFromJsonAsync<MemberRaRankHistoryResponse>(
            "/api/members/ShrimpPoboy/ra-rank-history");

        // Assert
        body.ShouldNotBeNull();
        body.Items.Count.ShouldBe(2);
        body.Items[0].Rank.ShouldBe(125_000);
        body.Items[0].TotalPoints.ShouldBe(500);
        body.Items[1].Rank.ShouldBe(120_000);
        body.Items[1].TotalTruePoints.ShouldBe(1_070);
    }
}
