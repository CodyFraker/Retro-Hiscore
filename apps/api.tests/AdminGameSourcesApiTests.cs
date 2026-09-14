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
public class AdminGameSourcesApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public AdminGameSourcesApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GameSourceCrud_WorksForAdmin()
    {
        // Arrange
        const int raGameId = 38130;
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act — create
        var createResponse = await admin.PostAsJsonAsync(
            $"/api/admin/games/{raGameId}/sources",
            new UpsertGameSourceRequest("Mega", "https://mega.nz/file/test", "Primary", 0, "USA"));
        var created = await createResponse.Content.ReadFromJsonAsync<GameSourceDto>(JsonOptions);

        // Assert — create
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        created.ShouldNotBeNull();
        created!.SourceType.ShouldBe("Mega");

        // Act — update
        var updateResponse = await admin.PutAsJsonAsync(
            $"/api/admin/games/{raGameId}/sources/{created.Id}",
            new UpsertGameSourceRequest("GoogleDrive", "https://drive.google.com/file/d/abc", "Mirror", 1, null));
        var updated = await updateResponse.Content.ReadFromJsonAsync<GameSourceDto>(JsonOptions);

        // Assert — update
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated!.SourceType.ShouldBe("GoogleDrive");
        updated.SortOrder.ShouldBe(1);

        // Act — list
        var list = await admin.GetFromJsonAsync<List<GameSourceDto>>(
            $"/api/admin/games/{raGameId}/sources",
            JsonOptions);

        // Assert — list
        list.ShouldNotBeNull();
        list!.Count.ShouldBe(1);

        // Act — delete
        var deleteResponse = await admin.DeleteAsync($"/api/admin/games/{raGameId}/sources/{created.Id}");

        // Assert — delete
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var afterDelete = await admin.GetFromJsonAsync<List<GameSourceDto>>(
            $"/api/admin/games/{raGameId}/sources",
            JsonOptions);
        afterDelete!.Count.ShouldBe(0);
    }

    [Fact]
    public async Task PostGameSource_ReturnsForbidden_ForNonAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/admin/games/38130/sources",
            new UpsertGameSourceRequest("Mega", "https://mega.nz/file/x", null, 0, null));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostGameSource_ReturnsValidationProblem_ForInvalidUrl()
    {
        // Arrange
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await admin.PostAsJsonAsync(
            "/api/admin/games/38130/sources",
            new UpsertGameSourceRequest("Mega", "http://insecure.example.com", null, 0, null));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteGame_RemovesSourcesViaCascade()
    {
        // Arrange
        const int raGameId = 2291;
        Guid sourceId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            var source = new GameSource
            {
                GameId = game.Id,
                SourceType = GameSourceType.Mediafire,
                Url = "https://www.mediafire.com/file/test"
            };
            db.GameSources.Add(source);
            await db.SaveChangesAsync();
            sourceId = source.Id;
        }

        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await admin.DeleteAsync($"/api/admin/games/{raGameId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.GameSources.AnyAsync(s => s.Id == sourceId)).ShouldBeFalse();
    }
}
