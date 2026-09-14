using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetGameSourcesApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public GetGameSourcesApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGameSources_ReturnsMirrors_ForMember()
    {
        // Arrange
        const int raGameId = 38130;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            db.GameSources.Add(new GameSource
            {
                GameId = game.Id,
                SourceType = GameSourceType.Mega,
                Url = "https://mega.nz/file/member-read",
                SortOrder = 0
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetAsync($"/api/games/{raGameId}/sources");
        var items = await response.Content.ReadFromJsonAsync<List<GameSourceDto>>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull();
        items!.Count.ShouldBe(1);
        items[0].Url.ShouldBe("https://mega.nz/file/member-read");
    }

    [Fact]
    public async Task GetGameSources_ReturnsNotFound_WhenGameMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/games/999999/sources");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
