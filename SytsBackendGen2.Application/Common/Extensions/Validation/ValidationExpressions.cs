using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.Validation;

internal static class ValidationExpressions
{
    public static IRuleBuilderOptions<T, string> MustBeExistingRole
        <T>(this IRuleBuilder<T, string> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => BeExistingRole(p, context))
            .WithMessage((q, p) => $"'{p} is not existing role'")
            .WithErrorCode("NotExistingRoleValidator");
    }

    public static IRuleBuilderOptions<T, int?> MustBeExistingRoleOrNull
        <T>(this IRuleBuilder<T, int?> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => BeExistingRoleOrNull(p, context))
            .WithMessage((q, p) => $"Role with id '{p}' does not exists")
            .WithErrorCode("NotExistingRoleValidator");
    }

    public static IRuleBuilderOptions<T, int> MustBeExistingPermission
        <T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder.Must((q, p) => BeExistingPermission(p))
            .WithMessage((q, p) => $"Permission with id '{p}' does not exists")
            .WithErrorCode("NotExistingRoleValidator");
    }

    private static bool BeExistingRole(string role, IAppDbContext context)
    {
        return context.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower()) != null;
    }

    private static bool BeExistingRoleOrNull(int? roleId, IAppDbContext context)
    {
        if (roleId == null)
            return true;
        return context.Roles.FirstOrDefault(r => r.Id == roleId) != null;
    }

    private static bool BeExistingPermission(string permissionName)
    {
        return Enum.TryParse<Permission>(permissionName, out var _);
    }

    private static bool BeExistingPermission(int permissionId)
    {
        return Enum.IsDefined(typeof(Permission), permissionId);
    }

    //private static bool BeValidEmail(string email)
    //{
    //    Regex regex = new Regex(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");
    //    return regex.IsMatch(email);
    //}

}
