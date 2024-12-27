using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Piazza.Database.Models;

namespace Piazza.API.Services;

public class JwtService(IConfiguration configuration)
{
    public string GenerateJWTToken(User user) {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };
        var jwtToken = new JwtSecurityToken(
            issuer: configuration["ApplicationSettings:Issuer"],
            audience: configuration["ApplicationSettings:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["ApplicationSettings:JWT_Secret"]!)
                ),
                SecurityAlgorithms.HmacSha256Signature)
        );
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GetIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["ApplicationSettings:JWT_Secret"]!);
        var claims = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return claims!.Claims.FirstOrDefault()!.Value;
    }
}