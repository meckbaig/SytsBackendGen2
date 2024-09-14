using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users
        => Set<User>();

    public DbSet<Role> Roles
        => Set<Role>();

    public DbSet<Access> Access
        => Set<Access>();

    public DbSet<Folder> Folders
        => Set<Folder>();

    public DbSet<PermissionInRole> PermissionsInRoles
        => Set<PermissionInRole>();

    public DbSet<UserCallToFolder> UsersCallsToFolders
        => Set<UserCallToFolder>();

    public DbSet<Permission> Permissions
        => Set<Permission>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.SetDeletedFilters();

        //builder.UseCustomFunctions();

        builder.Entity<Role>()
            .HasMany(o => o.Permissions)
            .WithMany(r => r.Roles)
            .UsingEntity<PermissionInRole>();

        builder.Entity<UserCallToFolder>()
            .HasKey(lv => new { lv.UserId, lv.FolderId });
        builder.Entity<UserCallToFolder>()
            .HasOne(lv => lv.User).WithMany(u => u.UserCallsToFolders)
            .HasForeignKey(v => v.UserId);
        builder.Entity<UserCallToFolder>()
            .HasOne(lv => lv.Folder).WithMany(v => v.UsersCallsToFolder)
            .HasForeignKey(v => v.FolderId);

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}


internal static class AppDbContextCustomFunctions
{
    /// <summary>
    /// Migrates all permissions into database.
    /// </summary>
    /// <param name="appDbContext">Data base context.</param>
    internal static void ConfigurePermissions(this IAppDbContext appDbContext)
    {
        HashSet<Permission> localPermissions = Enum.GetValues<Domain.Enums.Permission>()
            .Select(p => new Permission
            {
                Id = (int)p,
                Name = p.ToString()
            })
            .ToHashSet();

        HashSet<Permission> dbPermissions = appDbContext.Permissions.ToHashSet();

        foreach (var localPermission in localPermissions)
        {
            Permission? dbPermission = dbPermissions.FirstOrDefault(p => p.Id == localPermission.Id);
            if (dbPermission == null)
            {
                appDbContext.Permissions.Add(localPermission);
            }
            else
            {
                dbPermission.Name = localPermission.Name;
            }
        }

        HashSet<Permission> removePermissions = dbPermissions.Except(localPermissions, new PermissionsComparer()).ToHashSet();
        foreach (var removePermission in removePermissions)
        {
            appDbContext.Permissions.Remove(removePermission);
        }

        appDbContext.SaveChanges();
    }

    private class PermissionsComparer : IEqualityComparer<Permission>
    {
        public bool Equals(Permission? x, Permission? y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] Permission obj)
        {
            return obj.Id;
        }
    }

    internal static void SetDeletedFilters(this ModelBuilder builder)
    {
        var types = typeof(IAppDbContext)
            .GetProperties().Where(
                p => p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                typeof(INonDelitableEntity).IsAssignableFrom(
                    p.PropertyType.GetGenericArguments().FirstOrDefault()))
            .Select(p => p.PropertyType.GetGenericArguments().FirstOrDefault())
            .ToList();
        foreach (var type in types)
        {
            var methodInfo = typeof(AppDbContextCustomFunctions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == nameof(SetDeletedFilter));
            var genericMethod = methodInfo.MakeGenericMethod(type);
            object[] parameters = [builder];
            genericMethod.Invoke(null, parameters);
        }
    }

    private static void SetDeletedFilter<TEntity>(ModelBuilder builder)
        where TEntity : BaseEntity, INonDelitableEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(p => !p.Deleted);
    }

    // TODO: delete
    internal static void UseCustomFunctions(this ModelBuilder modelBuilder) // Does not work
    {
        var methodInfo = typeof(string).GetMethod(
            nameof(String.Equals),
            BindingFlags.Static | BindingFlags.Public,
            null,
            [typeof(string), typeof(string), typeof(StringComparison)],
            null);
        modelBuilder.Model.GetDefaultSchema();
        modelBuilder
            .HasDbFunction(methodInfo)
            .HasTranslation(args =>
            {
                StringComparison sc = (StringComparison)Convert.ToInt32(args[2].Print());
                switch (sc)
                {
                    case StringComparison.CurrentCulture:
                    case StringComparison.InvariantCulture:
                    case StringComparison.Ordinal:
                        return new SqlFunctionExpression(
                            "like",
                            args,
                            false,
                            args.Select(_ => false),
                            typeof(bool),
                            null);
                    case StringComparison.OrdinalIgnoreCase:
                    case StringComparison.InvariantCultureIgnoreCase:
                    case StringComparison.CurrentCultureIgnoreCase:
                        return new SqlFunctionExpression(
                            "ilike",
                            args,
                            false,
                            args.Select(_ => false),
                            typeof(bool),
                            null);
                }
                throw new NotImplementedException();
            });
    }
}