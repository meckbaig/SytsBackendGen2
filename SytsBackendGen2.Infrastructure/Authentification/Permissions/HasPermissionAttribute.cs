using Microsoft.AspNetCore.Authorization;
using SytsBackendGen2.Domain.Enums;
using SytsBackendGen2.Infrastructure.Authentification;

namespace SytsBackendGen2.Infrastructure.Authentification.Permissions;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission) : base(policy: permission.ToString())
    {

    }
}
