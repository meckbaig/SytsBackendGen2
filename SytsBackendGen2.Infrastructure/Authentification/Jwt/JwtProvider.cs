using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Entities.Authentification;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SytsBackendGen2.Infrastructure.Authentification.Jwt;

internal sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtProvider(JwtOptions options)
    {
        _options = options;
    }

    public string GenerateToken(
        User user,
        TimeSpan? tokenLifeTime = null,
        HashSet<Permission>? customPermissions = null)
    {
        var tokenHandler = new JsonWebTokenHandler();
#if RELEASE
        bool currentDevelopmentMode = false;
#else
                bool currentDevelopmentMode = true;
#endif

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(CustomClaim.UserId, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.Name),
            new(CustomClaim.DevelopmentMode, currentDevelopmentMode.ToString()),
        };
        foreach (var permission in customPermissions ?? user.Role.Permissions)
        {
            claims.Add(new("permissions", permission.Name));
        }

        var tokenDesctiptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(tokenLifeTime ?? TimeSpan.FromMinutes(_options.TokenLifetimeMinutes)),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };
        var token = tokenHandler.CreateToken(tokenDesctiptor);
        var jwt = tokenHandler.ReadJsonWebToken(token);

        return jwt.UnsafeToString();
    }

    public int GetUserIdFromClaimsPrincipal(ClaimsPrincipal principal)
    {
        string? idString = principal.Claims.FirstOrDefault(c => c.Type == CustomClaim.UserId)?.Value;
        if (idString != null && int.TryParse(idString, out int id))
            return id;
        throw new ArgumentException("JWT key does not contain user id");
    }

    public string GenerateRefreshToken(User user, string? token = null)
    {
        var randomNumber = new byte[64];

        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
        }

        string refreshToken = Convert.ToBase64String(randomNumber);

        if (token != null)
        {
            user.RefreshTokens
                .FirstOrDefault(t => t.Token.Equals(token))
                .Invalidated = true;
        }
        user.RefreshTokens
            .Add(new(refreshToken, DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(_options.RefreshTokenLifetimeDays))));


        return refreshToken;
    }
}

public static class CustomClaim
{
    public const string UserId = "userId";
    public const string Permissinos = "permissions";
    public const string DevelopmentMode = "developmentMode";
}