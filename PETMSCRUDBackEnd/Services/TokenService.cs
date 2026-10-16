using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PETMSCRUD.Models;

namespace PETMSCRUD.Services;

public class TokenService
{
    private readonly IConfiguration _cfg;
    public TokenService(IConfiguration cfg) => _cfg = cfg;

    public (string Token, DateTime Expires) Create(ApplicationUser user, IEnumerable<string> roles)
    {
        var jwt = _cfg.GetSection("Jwt");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),     // OwnerController reads this claim
            new(ClaimTypes.Email, user.Email ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));   // used by [Authorize(Roles=...)]

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var expires = DateTime.UtcNow.AddHours(double.Parse(jwt["ExpiryHours"] ?? "8"));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
