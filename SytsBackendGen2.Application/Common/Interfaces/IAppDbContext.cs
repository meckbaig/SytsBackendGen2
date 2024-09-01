using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IAppDbContext : IDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Access> Access { get; }
    DbSet<Folder> Folders { get; }
    DbSet<PermissionInRole> PermissionsInRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
}
