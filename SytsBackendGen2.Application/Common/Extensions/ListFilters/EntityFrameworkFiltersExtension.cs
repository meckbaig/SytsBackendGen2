using AutoMapper;
using AutoMapper.Internal;
using SytsBackendGen2.Application.Common.Attributes;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;
using SytsBackendGen2.Application.Extensions.JsonPatch;
using SytsBackendGen2.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SytsBackendGen2.Application.Extensions.ListFilters;

/// <summary>
/// Custom EF Core extencion class for dynamic filtering
/// </summary>
public static class EntityFrameworkFiltersExtension
{
    /// <summary>
    /// Adds 'Where' statements using input filters and mapping engine
    /// </summary>
    /// <typeparam name="TSource">Source of DTO type</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="filterExpressions">Array of filter expressions</param>
    /// <returns>An <typeparamref name="IQueryable"/> that contains filters</returns>
    public static IQueryable<TSource> AddFilters<TSource>
        (this IQueryable<TSource> source, List<Expression>? filterExpressions)
        where TSource : BaseEntity
    {
        if (filterExpressions == null)
            return source;
        foreach (var expression in filterExpressions)
        {
            source = source.Where((Expression<Func<TSource, bool>>)expression);
        }
        return source;
    }

    /// <summary>
    /// Gets full endpoint route string.
    /// </summary>
    /// <param name="destinationPropertyName"></param>
    /// <param name="provider">Configuraion provider for performing maps.</param>
    /// <param name="destinationType">DTO type.</param>
    /// <param name="propertyType">Next property type, if property is collection; else null.</param>
    /// <returns>Returns endpoint if success, null if error</returns>
    public static string? GetExpressionEndpoint
        (string destinationPropertyName, IConfigurationProvider provider, Type destinationType, out Type propertyType)
    {
        propertyType = destinationType;
        DtoExtension.InvokeTryGetSource(
                    destinationPropertyName,
                    provider,
                    ref propertyType,
                    out string sourceSegment,
                    out string errorMessage,
                    throwException: false);
        if (propertyType.IsCollection())
            propertyType = propertyType.GetGenericArguments().Single();
        else
            propertyType = null;

        return sourceSegment;
    }

    /// <summary>
    /// Gets filter expression
    /// </summary>
    /// <typeparam name="TDestintaion">DTO type</typeparam>
    /// <param name="filter">Filter from client</param>
    /// <param name="provider">Configuraion provider for performing maps</param>
    /// <returns>Returns FilterExpression model if success, undefined FilterExpression
    /// if can not parse expression, null if error</returns>
    public static FilterExpression? GetFilterExpression
        <TDestintaion>
        (string filter, IConfigurationProvider provider)
        where TDestintaion : class, IBaseDto
    {
        try
        {
            return FilterExpression.Initialize<TDestintaion>(filter, provider);
        }
        catch (ValidationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets filter attribute from property path.
    /// </summary>
    /// <typeparam name="TDestintaion">DTO type.</typeparam>
    /// <param name="propertyPath">Property path to get attribute from.</param>
    /// <returns>Returns FilterableAttribute models for full path if success, null if error.</returns>
    public static bool TryGetFilterAttributes<TDestintaion>
        (FilterExpression filterEx, out string key)
        where TDestintaion : IBaseDto
    {
        var tmpFilterEx = filterEx;
        Type nextSegmentType = typeof(TDestintaion);
        do
        {
            key = tmpFilterEx.Key;
            var prop = nextSegmentType.GetProperties().FirstOrDefault(p => p.Name == tmpFilterEx.Key)!;

            var attribute = (FilterableAttribute)prop.GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(FilterableAttribute))!;
            if (attribute == null)
                return false;

            tmpFilterEx.CompareMethod = attribute.CompareMethod;

            if (tmpFilterEx.CompareMethod == CompareMethod.Nested)
                nextSegmentType = prop.PropertyType.GetGenericArguments().Single();
            else
                nextSegmentType = prop.PropertyType;

            tmpFilterEx = tmpFilterEx.InnerFilterExpression;

        } while (tmpFilterEx != null);

        return true;
    }

    /// <summary>
    /// Gets filter expression for Where statement.
    /// </summary>
    /// <param name="filterEx">Filter expression.</param>
    /// <param name="expression">Filter expression if success, null if error.</param>
    /// <returns><see langword="true"/> if success, <see langword="false"/> if error.</returns>
    public static bool TryGetLinqExpression
        (FilterExpression filterEx, out Expression expression)
    {
        var param = Expression.Parameter(filterEx.EntityType, filterEx.EntityType.Name.ToCamelCase());

        string[] endpointSegments = filterEx.EndPoint.Split('.');
        MemberExpression propExpression = Expression.Property(param, endpointSegments[0]);
        if (endpointSegments.Length != 1)
        {
            for (int i = 1; i < endpointSegments.Length; i++)
            {
                propExpression = Expression.Property(propExpression, endpointSegments[i]);
            }
        }

        object[] values = filterEx.Value.Split(',');
        switch (filterEx.CompareMethod)
        {
            case CompareMethod.Equals:
                expression = EqualExpression(values, propExpression, filterEx.ExpressionType);
                break;
            case CompareMethod.ById:
                expression = ByIdExpression(values, propExpression, filterEx.ExpressionType);
                break;
            case CompareMethod.Nested:
                expression = NestedExpression(propExpression, filterEx);
                break;
            default:
                expression = null;
                break;
        }
        if (expression != null)
            expression = InvokeFilterLambda(param, expression, filterEx.EntityType);
        return expression != null;
    }

    /// <summary>
    /// Gets Where statement with inner filter expression.
    /// </summary>
    /// <param name="values">Filter strings.</param>
    /// <param name="propExpression">A field of property.</param>
    /// <param name="filterEx">Filter expression.</param>
    /// <returns>Lambda expression with Where() statement and inner expression.</returns>
    /// <remarks>Output expression example: <code>x.EntitiesList.Where(e => e.Property == value).Count() != 0</code></remarks>
    private static Expression NestedExpression(MemberExpression propExpression, FilterExpression filterEx)
    {

        if (!TryGetLinqExpression(filterEx.InnerFilterExpression, out var filterLambda))
            return null;

        MethodInfo whereMethod = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == "Where")
            .First(m => m.GetParameters().Length == 2)
            .MakeGenericMethod(filterEx.InnerFilterExpression.EntityType);

        // Create Where expressiom
        var whereCallExpression = Expression.Call(
            whereMethod,
            propExpression,
            filterLambda);

        // Get method Count for IEnumerable<T>
        MethodInfo countMethod = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == "Count" && m.GetParameters().Length == 1)
            .Single()
            .MakeGenericMethod(filterEx.InnerFilterExpression.EntityType);

        // Create Count expression
        var countCallExpression = Expression.Call(
            countMethod,
            whereCallExpression);

        // Create expression for comparison with zero 
        var zeroExpression = Expression.Constant(0);
        var notEqualExpression = Expression.NotEqual(countCallExpression, zeroExpression);

        return notEqualExpression;
    }

    /// <summary>
    /// Filter lambda call with spectified type.
    /// </summary>
    /// <param name="param">Parameter expression to create lambda.</param>
    /// <param name="expression">Expression for lambda.</param>
    /// <param name="sourceType">Source data type.</param>
    /// <returns>Lambda expression</returns>
    private static Expression InvokeFilterLambda(ParameterExpression param, Expression expression, Type sourceType)
    {
        Type yourType = typeof(EntityFrameworkFiltersExtension);

        MethodInfo methodInfo = yourType.GetMethod(nameof(GetFilterLambda), BindingFlags.NonPublic | BindingFlags.Static);

        MethodInfo genericMethod = methodInfo.MakeGenericMethod(sourceType);

        return (Expression)genericMethod.Invoke(null, [param, expression]);
    }

    /// <summary>
    /// Filter lambda call.
    /// </summary>
    /// <typeparam name="TSource">Source data type.</typeparam>
    /// <param name="param">Parameter expression to create lambda.</param>
    /// <param name="expression">Expression for lambda.</param>
    /// <returns>Lambda expression</returns>
    private static Expression<Func<TSource, bool>> GetFilterLambda<TSource>(ParameterExpression param, Expression expression)
    {
        return Expression.Lambda<Func<TSource, bool>>(expression, param);
    }

    /// <summary>
    /// Creates Equal() lambda expression from array of filter strings
    /// </summary>
    /// <param name="values">Filter strings</param>
    /// <param name="propExpression">A field of property</param>
    /// <param name="expressionType">Type of expression</param>
    /// <returns>Lambda expression with Equal() filter</returns>
    private static Expression EqualExpression
        (object[] values, MemberExpression propExpression, FilterExpressionType expressionType)
    {
        if (values.Length == 0)
            return Expression.Empty();
        Expression expression = Expression.Empty();
        for (int i = 0; i < values.Length; i++)
        {
            if (i == 0)
                expression = GetSingleEqualExpression(values[i], propExpression);
            else
                expression = Expression.OrElse(expression,
                    GetSingleEqualExpression(values[i], propExpression));
        }
        if (expressionType == FilterExpressionType.Include)
            return expression;
        else
            return Expression.Not(expression);
    }


    private static BinaryExpression GetSingleEqualExpression(object value, MemberExpression propExpression)
    {
        if (value.ToString()!.Contains(".."))
        {
            string valueString = value.ToString();
            object from = ConvertFromObject(
                valueString.Substring(0, valueString.IndexOf("..")), propExpression.Type);
            object to = ConvertFromObject(
                valueString.Substring(valueString.IndexOf("..") + 2), propExpression.Type);
            List<BinaryExpression> binaryExpressions = new List<BinaryExpression>();
            if (from != null)
                binaryExpressions.Add(Expression.GreaterThanOrEqual(
                    propExpression, Expression.Constant(from, propExpression.Type)));
            if (to != null)
                binaryExpressions.Add(Expression.LessThanOrEqual(
                    propExpression, Expression.Constant(to, propExpression.Type)));
            switch (binaryExpressions.Count)
            {
                case 2:
                    return Expression.AndAlso(binaryExpressions[0], binaryExpressions[1]);
                case 1:
                    return binaryExpressions[0];
                default:
                    throw new Exception($"Could not translate expression {valueString}");
            }
        }
        else
        {
            return Expression.Equal(
                propExpression,
                Expression.Constant(
                    ConvertFromObject(value, propExpression.Type),
                    propExpression.Type));
        }
    }

    /// <summary>
    /// Creates Equal() lambda expression by id from array of filter strings
    /// </summary>
    /// <param name="values">Filter strings</param>
    /// <param name="propExpression">A field of property</param>
    /// <param name="expressionType">Type of expression</param>
    /// <returns>Lambda expression with Equal() filter by id</returns>
    private static Expression ByIdExpression
        (object[] values, MemberExpression propExpression, FilterExpressionType expressionType)
    {
        string? key = GetForeignKeyFromModel(propExpression.Expression.Type, propExpression.Member.Name);

        if (key != null)
            propExpression = Expression.Property(propExpression.Expression, key);
        else
            throw new NotImplementedException("Not supported operation");

        return EqualExpression(values, propExpression, expressionType);
    }

    private static string? GetForeignKeyFromModel(Type type, string modelName)
    {
        PropertyInfo? property = type.GetProperties().FirstOrDefault(
            p => ((ForeignKeyAttribute)p.GetCustomAttributes(true)
            .FirstOrDefault(a => a.GetType() == typeof(ForeignKeyAttribute)))?.Name == modelName)!;
        if (property == null)
        {
            string idPropertyName = type.GetProperties()
                .FirstOrDefault(p => p.Name == modelName)
                .GetCustomAttribute<ForeignKeyAttribute>()
                .Name;
            property = type.GetProperties()
                .FirstOrDefault(p => p.Name == idPropertyName);
        }
        return property?.Name;
    }

    private static object ConvertFromString(this string value, Type type)
    {
        if (value == "")
            return null;
        if (type == typeof(DateOnly) || type == typeof(DateOnly?))
            return DateOnly.Parse(value);
        return Convert.ChangeType(value, type);
    }

    private static object ConvertFromObject(object value, Type type)
        => value.ToString().ConvertFromString(type);
}