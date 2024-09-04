using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Extensions.DataBaseProvider;

public static class QueryExtensions
{
    /// <returns>User with role (including permiissions)</returns>
    public static User? WithRoleByEmail(this IQueryable<User> users, string email)
    {
        return users
            .Include(u => u.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefault(k => k.Email == email);
    }

    /// <returns>User with role (including permiissions)</returns>
    public static User WithRoleById(this IQueryable<User> users, int id)
    {
        return users.Include(u => u.Role).ThenInclude(r => r.Permissions)
            .FirstOrDefault(k => k.Id == id);
    }
}
