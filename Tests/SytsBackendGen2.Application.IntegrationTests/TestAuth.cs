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

namespace SytsBackendGen2.Application.SystemTests;

internal static class TestAuth
{
    public const string MainUserToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJTeXRzQmFja2VuZFVzZXIiLCJpc3MiOiJTeXRzQmFja2VuZEdlbjIiLCJleHAiOjIwMjYyMDk4NTAsImp0aSI6ImRlN2Q1ZTE2LTQzOTUtNGMzNy04YTlhLTA5NzVhYzYzYzY3ZCIsImVtYWlsIjoidGhlY2VtZW4yMjhAZ21haWwuY29tIiwidXNlcklkIjoiMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlJlZ3VsYXJVc2VyIiwiZGV2ZWxvcG1lbnRNb2RlIjoiVHJ1ZSIsInBlcm1pc3Npb25zIjpbIlB1YmxpY0RhdGFSZWFkZXIiLCJQcml2YXRlRGF0YUVkaXRvciJdLCJpYXQiOjE3MjYyMDk4NTAsIm5iZiI6MTcyNjIwOTg1MH0.chW6QYSaAd0nJbTB2JKTP06VOE7TR3z93T3fb8ydRPE";
    public const string SecondUserToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJTeXRzQmFja2VuZFVzZXIiLCJpc3MiOiJTeXRzQmFja2VuZEdlbjIiLCJleHAiOjIwMjYyMDk4ODYsImp0aSI6ImY2N2M2YmI1LTNmOGQtNDkzNy1iY2RkLTYyMzBmMzQ4MTgzNyIsImVtYWlsIjoibWVja2JhaWdAeWFuZGV4LnJ1IiwidXNlcklkIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlJlZ3VsYXJVc2VyIiwiZGV2ZWxvcG1lbnRNb2RlIjoiVHJ1ZSIsInBlcm1pc3Npb25zIjpbIlB1YmxpY0RhdGFSZWFkZXIiLCJQcml2YXRlRGF0YUVkaXRvciJdLCJpYXQiOjE3MjYyMDk4ODYsIm5iZiI6MTcyNjIwOTg4Nn0.rJ7julfDTI6vUie0UAu-6GD8Q2Ab9xxcjCK7e6jTyeg";
    //public static string GetToken(params Domain.Enums.Permission[] permissions)
    //{
    //    HashSet<Permission> permissionsSet = permissions
    //        .Select(p => (Permission)p).ToHashSet();

    //    string newPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\SytsBackendGen2.Web"));

    //    var builder = new ConfigurationBuilder()
    //        .SetBasePath(newPath)
    //        .AddJsonFile("appsettings.json");

    //    var configuration = builder.Build();

    //    var jwtOptions = new JwtOptions();
    //    configuration.GetSection(JwtOptionsSetup.SectionName).Bind(jwtOptions);

    //    User user = new()
    //    {
    //        Name = "Tester",
    //        Id = int.MaxValue,
    //        Role = new() { Id = int.MaxValue, Name = "TestRole" }
    //    };

    //    JwtProvider provider = new(jwtOptions);
    //    return provider.GenerateToken(user, customPermissions: permissionsSet);
    //}
}
