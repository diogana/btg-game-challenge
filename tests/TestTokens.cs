using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
internal static class TestTokens
{
    public static string Create(string key, bool role, bool scope)
    {
        var claims = new List<Claim> { new("sub", "tests"), new("client_id", "tests") };
        if (role) claims.Add(new("roles", "GameReader")); if (scope) claims.Add(new("scope", "games.read"));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("GameLending.Auth", "GameLending", claims,
            DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddMinutes(5), new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
    }
}
