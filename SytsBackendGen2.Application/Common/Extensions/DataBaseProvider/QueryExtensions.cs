using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Extensions.DataBaseProvider;

public static class QueryExtensions
{
    /// <returns>User with role (including permiissions)</returns>
    public static async Task<User?> WithRoleByEmailAsync(this IQueryable<User> users, string email, CancellationToken cancellationToken = default)
    {
        return await users
            .Include(u => u.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(k => k.Email == email, cancellationToken);
    }

    /// <returns>User with role (including permiissions)</returns>
    public static async Task<User?> WithRoleByIdAsync(this IQueryable<User> users, int id, CancellationToken cancellationToken = default)
    {
        return await users.Include(u => u.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a folder with its associated access information from the database based on the provided folder guid.
    /// </summary>
    /// <param name="folders">The queryable collection of folders.</param>
    /// <param name="guid">The guid of the folder to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The folder with its associated access information, or null if no folder is found.</returns>
    public static async Task<Folder?> WithAccessByGuidAsync(this IQueryable<Folder> folders, Guid guid, CancellationToken cancellationToken = default)
    {
        return await folders
            .Include(f => f.Access)
            .FirstOrDefaultAsync(k => k.Guid == guid, cancellationToken);
    }

    /// <summary>
    /// Extends the IQueryable of Folder objects by including the UsersCallsToFolder
    /// where the UserId matches the provided userId.
    /// </summary>
    /// <param name="folders">The IQueryable of Folder objects to be extended.</param>
    /// <param name="userId">The userId to match the UserId of the UsersCallsToFolder.</param>
    /// <returns>IQueryable of Folder objects with the UsersCallsToFolder for the provided userId included.</returns>
    public static IQueryable<Folder> WithUserCall(this IQueryable<Folder> folders, int userId)
    {
        return folders.Include(f => f.UsersCallsToFolder.Where(uc => uc.UserId == userId)).AsQueryable();
    }
}