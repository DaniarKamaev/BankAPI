using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankAPI.Features.Authentication;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/login", async ([FromBody] LoginRequest request, IConfiguration config) =>
        {
            if (request.Username != "admin" || request.Password != "admin123")
                return Results.Unauthorized();

            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var key = config["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
                return Results.Problem("JWT Key is not configured.");

            var keyBytes = Encoding.ASCII.GetBytes(key);
            if (keyBytes.Length < 16)
                return Results.Problem("JWT Key must be at least 16 bytes long for HMAC-SHA512.");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                    new Claim(JwtRegisteredClaimNames.Email, request.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Results.Ok(new { Token = jwtToken });
        });
    }
}

public record LoginRequest(string Username, string Password);
