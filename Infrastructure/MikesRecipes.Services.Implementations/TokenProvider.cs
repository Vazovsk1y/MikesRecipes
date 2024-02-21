using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations;

public class TokenProvider(IOptions<JwtAuthOptions> jwtOptions, IClock clock) : ITokenProvider
{
    private readonly JwtAuthOptions _jwtOptions = jwtOptions.Value;
    private readonly IClock _clock = clock;
    public string GenerateAccessToken(GenerateAccessTokenDTO generateAccessTokenDTO)
    {
        var expiredDate = _clock.GetUtcNow().AddMinutes(_jwtOptions.JwtTokenLifetimeMinutesCount);
        var roles = generateAccessTokenDTO.Roles.Select(e => new Claim(ClaimTypes.Role, e));

        var claims = new List<Claim>(roles)
        {
            new(JwtRegisteredClaimNames.Name, generateAccessTokenDTO.Username),
            new(JwtRegisteredClaimNames.Email, generateAccessTokenDTO.Email),
            new(JwtRegisteredClaimNames.Sub, generateAccessTokenDTO.UserId.ToString()),
            new(JwtRegisteredClaimNames.Exp, expiredDate.ToString()),
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256
            );

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            null,
            expiredDate.DateTime,
            signingCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var randomGenerator = RandomNumberGenerator.Create();
        randomGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
public record JwtAuthOptions
{
    public const string SectionName = "Jwt";
    public required string Audience { get; init; }
    public required string Issuer { get; init; }
    public required string SecretKey { get; init; }
    public required int SkewMinutesCount { get; init; }
    public required int JwtTokenLifetimeMinutesCount { get; init; }
    public required int RefreshTokenLifetimeDaysCount { get; init; }
}