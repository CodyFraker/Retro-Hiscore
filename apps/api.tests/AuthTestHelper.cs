using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RetroHiscore.Api.Tests;

public static class AuthTestHelper
{
    public const string TestSigningKey = "test-signing-key-for-integration-tests-only";
    public const string AllowedDiscordUserId = "123456789012345678";
    public const string DeniedDiscordUserId = "987654321098765432";

    public static string CreateToken(string discordUserId, string username = "testuser")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim("sub", discordUserId),
                new Claim("name", username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
