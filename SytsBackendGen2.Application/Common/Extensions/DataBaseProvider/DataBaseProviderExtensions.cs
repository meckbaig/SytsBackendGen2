using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.Interfaces;
using System.Reflection;

namespace SytsBackendGen2.Application.Extensions.DataBaseProvider;

internal static class DataBaseProviderExtensions
{
    /// <summary>
    /// Gets DbSet with selected  type from DbContext in the DbSet that called the method.
    /// </summary>
    /// <typeparam name="T">Type of generic in the DbSet.</typeparam>
    /// <param name="dbSet">The DbSet from which the context will be taken</param>
    /// <param name="entityType">Type of generic in the result DbSet.</param>
    /// <param name="queryable">Result DbSet.</param>
    /// <returns><see langword="true"/> if <paramref name="queryable" /> was found successfully; otherwise, false.</returns>
    public static bool TryGetDbSetFromAnotherDbSet<T>(this DbSet<T> dbSet, Type entityType, out IQueryable queryable)
        where T : class
    {
        FieldInfo fieldInfo = dbSet.GetType().GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isDbSet = fieldInfo != null;
        //Type dbSetType = typeof(DbSet<>).MakeGenericType(entityType);
        //bool isDbSet = typeof(IAppDbContext)
        //    .GetProperties()
        //    .Any(p => p.PropertyType.IsGenericType &&
        //              p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
        //              p.PropertyType == dbSetType);
        if (isDbSet)
        {
            var context = (IDbContext)fieldInfo.GetValue(dbSet);
            queryable = context.CreateDbSet(entityType);
        }
        else
        {
            queryable = null;
        }
        return isDbSet;
    }

    internal static IQueryable CreateDbSet(this IDbContext context, Type elementType)
    {
        MethodInfo setMethod = typeof(IDbContext)
            .GetMethod(nameof(IDbContext.Set))
            .MakeGenericMethod(elementType);

        return (IQueryable)setMethod.Invoke(context, null);
    }

    /// <summary>
    /// Gets context from DbSet.
    /// </summary>
    /// <typeparam name="T">Type of generic in the DbSet.</typeparam>
    /// <param name="dbSet">The DbSet from which the context will be taken.</param>
    /// <returns>DB context.</returns>
    public static IDbContext GetContext<T>(this DbSet<T> dbSet) where T : class
    {
        FieldInfo fieldInfo = dbSet.GetType().GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IDbContext)fieldInfo.GetValue(dbSet);
    }

    /// <summary>
    /// Possibility of getting DbSet with selected type.
    /// </summary>
    /// <param name="interfaceType">DbContext interface.</param>
    /// <param name="entityType">Type of generic in the result DbSet.</param>
    /// <returns><see langword="true"/> if DbSet was found; otherwise, false.</returns>
    public static bool CanGetDbSet(this Type interfaceType, Type entityType)
    {
        Type dbSetType = typeof(DbSet<>).MakeGenericType(entityType);
        return interfaceType
            .GetProperties()
            .Any(p => p.PropertyType.IsGenericType &&
                      p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                      p.PropertyType == dbSetType);
    }
}
