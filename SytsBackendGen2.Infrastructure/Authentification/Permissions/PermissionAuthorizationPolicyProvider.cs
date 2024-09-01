using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Infrastructure.Authentification.Permissions;

public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy != null)
            return policy;

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
