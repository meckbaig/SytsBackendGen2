using AutoMapper;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Extensions.JsonPatch;
using SytsBackendGen2.Domain.Common;
using System.Linq.Expressions;

namespace SytsBackendGen2.Application.Extensions.ListFilters;

/// <summary>
/// Custom EF Core extension class for dynamic sorting
/// </summary>
public static class EntityFrameworkOrderByExtension
{
    /// <summary>
    /// Adds 'OrderBy' statements using input sort filters and mapping engine
    /// </summary>
    /// <typeparam name="TSource">Source of DTO type</typeparam>
    /// <typeparam name="TDestintaion">DTO type</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="orderByExpressions">Array of sort expressions</param>
    /// <returns>An <typeparamref name="IOrderedQueryable"/> that contains sorting</returns>
    public static IOrderedQueryable<TSource> AddOrderBy<TSource>
        (this IQueryable<TSource> source, List<Expression>? orderByDelegates)
        where TSource : BaseEntity
    {
        if (orderByDelegates == null || orderByDelegates.Count() == 0)
            return source.OrderBy(x => x.Id);

        IOrderedQueryable<TSource> result = source.OrderBy(x => 0);

        foreach (var orderByDelegate in orderByDelegates)
        {
            var func = (Expression<Func<IOrderedQueryable<TSource>, IOrderedQueryable<TSource>>>)orderByDelegate;
            result = func.Compile()(result);
        }

        return result;
    }

    public static bool TryGetLinqExpression<TSource>(
        OrderByExpression orderByEx,
        out Expression? expression)
        where TSource : BaseEntity
    {
        var param = Expression.Parameter(typeof(TSource), "x");

        string[] endpoint = orderByEx.EndPoint.Split('.');
        MemberExpression propExpression = Expression.Property(param, endpoint[0]);
        if (endpoint.Length != 1)
        {
            for (int i = 1; i < endpoint.Length; i++)
            {
                propExpression = Expression.Property(propExpression, endpoint[i]);
            }
        }

        var func = typeof(Func<,>);
        var genericFunc = func.MakeGenericType(typeof(TSource), propExpression.Type);
        var lambda = Expression.Lambda(genericFunc, propExpression, param);

        Expression<Func<TSource, object>> keySelector = Expression.Lambda<Func<TSource, object>>(lambda, param);

        Func<IOrderedQueryable<TSource>, IOrderedQueryable<TSource>> orderByDelegate;

        string? expressionName;
        switch (orderByEx.ExpressionType)
        {
            case OrderByExpressionType.Ascending:
                expressionName = nameof(Queryable.ThenBy);
                break;
            case OrderByExpressionType.Descending:
                expressionName = nameof(Queryable.ThenByDescending);
                break;
            default:
                expressionName = null;
                break;
        }

        if (expressionName == null)
        {
            expression = null;
            return false;
        }

        var sourceParam = Expression.Parameter(typeof(IOrderedQueryable<TSource>), "source");
        var thing = Expression.Call(
                typeof(Queryable),
                expressionName,
                [typeof(TSource), propExpression.Type],
                sourceParam,
                Expression.Quote(lambda));

        expression = Expression.Lambda<Func<IOrderedQueryable<TSource>, IOrderedQueryable<TSource>>>(thing, sourceParam);
        return true;
    }




    /// <summary>
    /// Gets full endpoint route string
    /// </summary>
    /// <typeparam name="TSource">Source of DTO type</typeparam>
    /// <typeparam name="TDestintaion">DTO type</typeparam>
    /// <param name="destinationPropertyName">Source prioer</param>
    /// <param name="provider">Configuraion provider for performing maps</param>
    /// <returns>Returns endpoint if success, null if error</returns>
    public static string GetExpressionEndpoint<TSource, TDestintaion>
        (string destinationPropertyName, IConfigurationProvider provider)
    {
        return DtoExtension.GetSource<TSource, TDestintaion>
            (destinationPropertyName, provider, throwException: false);
    }

    /// <summary>
    /// Gets sorting expression
    /// </summary>
    /// <typeparam name="TSource">Source of DTO type</typeparam>
    /// <typeparam name="TDestintaion">DTO type</typeparam>
    /// <param name="sortingExpressionString">Sorting expression from client</param>
    /// <param name="provider">Configuraion provider for performing maps</param>
    /// <returns>Returns OrderByExpression model if success, undefined OrderByExpression
    /// if can not parse expression, null if error</returns>
    internal static OrderByExpression GetOrderByExpression
        <TSource, TDestintaion>
        (string sortingExpressionString, IConfigurationProvider provider)
        where TSource : BaseEntity
        where TDestintaion : IBaseDto
    {
        try
        {
            return OrderByExpression.Initialize<TSource, TDestintaion>
                (sortingExpressionString, provider);
        }
        catch (ValidationException)
        {
            return null;
        }
    }
}
