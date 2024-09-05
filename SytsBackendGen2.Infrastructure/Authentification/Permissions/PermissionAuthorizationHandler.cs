using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SytsBackendGen2.Infrastructure.Authentification.Permissions;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        //string expString = context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value;
        //if (!long.TryParse(expString, out long expiresInSeconds))
        //    return Task.CompletedTask;
        //DateTime expires = DateTimeOffset.FromUnixTimeSeconds(expiresInSeconds).UtcDateTime;
        //if (expires < DateTime.UtcNow)
        //    return Task.CompletedTask;

        HashSet<string> permissions = context.User.Claims
            .Where(x => x.Type == "permissions")
            .Select(selector => selector.Value)
            .ToHashSet();
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public interface IPermissionService
{
    Task<HashSet<string>> GetPermissionsAsync();
}