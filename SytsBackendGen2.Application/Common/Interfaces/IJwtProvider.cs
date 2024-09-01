using SytsBackendGen2.Domain.Entities.Authentification;
using System.Security.Claims;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IJwtProvider
{
    /// <summary>
    /// Gets refresh token lifetime.
    /// </summary>
    /// <returns>Refresh token lifetime.</returns>
    TimeSpan GetRefreshTokenLifeTime();

    /// <summary>
    /// Generates JWT for provided user.
    /// </summary>
    /// <param name="user">User for new token validation.</param>
    /// <param name="tokenLifeTime">Custom token lifetime.</param>
    /// <param name="customPermissions">Custom permissions for token.</param>
    /// <returns>Json web token.</returns>
    string GenerateToken(User user, TimeSpan? tokenLifeTime = null, HashSet<Permission>? customPermissions = null);

    /// <summary>
    /// Generates refresh token.
    /// </summary>
    /// <param name="user">User for new refresh token validation.</param>
    /// <param name="token">Previous refresh token.</param>
    /// <returns>New refresh token.</returns>
    string GenerateRefreshToken(User user, string? token = null);

    /// <summary>
    /// Gets user id from plaims principal.
    /// </summary>
    /// <param name="principal">Claims principal to take id from.</param>
    /// <returns>User id.</returns>
    int GetUserIdFromClaimsPrincipal(ClaimsPrincipal principal);
}
