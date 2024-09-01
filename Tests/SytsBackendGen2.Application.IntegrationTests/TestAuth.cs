using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Entities.Authentification;
using SytsBackendGen2.Web.Structure.OptionsSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using SytsBackendGen2.Infrastructure.Authentification.Jwt;

namespace SytsBackendGen2.Application.IntegrationTests;

internal static class TestAuth
{
    public static string GetToken(params Domain.Enums.Permission[] permissions)
    {
        HashSet<Permission> permissionsSet = permissions
            .Select(p => (Permission)p).ToHashSet();

        string newPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\SytsBackendGen2.Web"));

        var builder = new ConfigurationBuilder()
            .SetBasePath(newPath)
            .AddJsonFile("appsettings.json");

        var configuration = builder.Build();

        var jwtOptions = new JwtOptions();
        configuration.GetSection(JwtOptionsSetup.SectionName).Bind(jwtOptions);

        User user = new()
        {
            Name = "Tester",
            Id = int.MaxValue,
            Role = new() { Id = int.MaxValue, Name = "TestRole" }
        };

        JwtProvider provider = new(jwtOptions);
        return provider.GenerateToken(user, customPermissions: permissionsSet);
    }
}
