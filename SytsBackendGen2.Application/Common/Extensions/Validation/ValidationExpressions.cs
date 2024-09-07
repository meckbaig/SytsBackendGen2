using FluentValidation;
using SytsBackendGen2.Application.Common.Interfaces;

namespace SytsBackendGen2.Application.Extensions.Validation;

internal static class ValidationExpressions
{
    public static IRuleBuilderOptions<T, int> MustHaveValidUserId
        <T>(this IRuleBuilder<T, int> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => HaveValidUserId(p, context))
            .WithMessage($"User Id is not valid")
            .WithErrorCode("NotValidUserId");
    }

    private static bool HaveValidUserId(int userId, IAppDbContext context)
    {
        if (userId > 0)
            return context.Users.Any(u => u.Id == userId);
        return false;
    }
    public static IRuleBuilderOptions<T, Guid> MustHaveValidFolderGuid
        <T>(this IRuleBuilder<T, Guid> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => HaveValidFolderGuid(p, context))
            .WithMessage($"Folder guid is not valid")
            .WithErrorCode("NotValidFolderGuid");
    }

    private static bool HaveValidFolderGuid(Guid folderGuid, IAppDbContext context)
    {
        return context.Folders.Any(u => u.Guid == folderGuid);
    }
}
