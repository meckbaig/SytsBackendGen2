namespace SytsBackendGen2.Infrastructure.Authentification.Jwt;

public class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SecretKey { get; init; }
    public int TokenLifetimeMinutes { get; init; }
    public int RefreshTokenLifetimeDays { get; init; }
}
