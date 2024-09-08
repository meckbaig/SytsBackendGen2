using FluentValidation;
using SytsBackendGen2.Application.Common.Interfaces;

namespace SytsBackendGen2.Application.Extensions.Validation;

internal static class ValidationExpressions
{
    public static IRuleBuilderOptions<T, int> MustHaveValidUserId
        <T>(this IRuleBuilder<T, int> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => HaveValidUserId(p, context))
            .WithMessage($"User with provided id doesn't exist.")
            .WithErrorCode("UserDoesNotExist");
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
            .WithMessage($"Folder with provided guid doesn't exist.")
            .WithErrorCode("FolderDoesNotExist");
    }

    private static bool HaveValidFolderGuid(Guid folderGuid, IAppDbContext context)
    {
        return context.Folders.Any(u => u.Guid == folderGuid);
    }
}
