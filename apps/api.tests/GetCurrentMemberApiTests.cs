using System.Net.Http.Json;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GetCurrentMemberApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetCurrentMemberApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCurrentMember_ReturnsOnboardingStep()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var member = await client.GetFromJsonAsync<CurrentMemberDto>("/api/members/me");

        // Assert
        member.ShouldNotBeNull();
        member.OnboardingStep.ShouldBe("NeedsApiKey");
        member.UiTheme.ShouldBe(UiThemes.Steam);
    }
}
