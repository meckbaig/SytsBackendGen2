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
}