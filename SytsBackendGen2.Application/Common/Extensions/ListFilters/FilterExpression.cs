using AutoMapper;
using SytsBackendGen2.Application.Common.Attributes;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;
using SytsBackendGen2.Application.Extensions.JsonPatch;

namespace SytsBackendGen2.Application.Extensions.ListFilters;

/// <summary>
/// Class representing a filtering expression
/// </summary>
/// <typeparam name="TSource">Source of DTO type</typeparam>
/// <typeparam name="TDestintaion">DTO type</typeparam>
public record FilterExpression : IEntityFrameworkExpression<FilterExpressionType>
{
    /// <summary>
    /// DTO key
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Source endpoint key
    /// </summary>
    public string? EndPoint { get; set; }

    /// <summary>
    /// Type of entity to use expression to
    /// </summary>
    public Type EntityType { get; set; }

    /// <summary>
    /// Type of expression
    /// </summary>
    public FilterExpressionType ExpressionType { get; set; }

    /// <summary>
    /// Filter value
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Nested filter expression (if property is a collection)
    /// </summary>
    public FilterExpression? InnerFilterExpression { get; set; }

    /// <summary>
    /// Entity compare method
    /// </summary>
    public CompareMethod CompareMethod { get; set; } = CompareMethod.Undefined;

    /// <summary>
    /// Factory constructor.
    /// </summary>
    /// <typeparam name="TDestintaion">DTO type.</typeparam>
    /// <param name="filter">Filter string.</param>
    /// <param name="provider">Configuraion provider for performing maps.</param>
    /// <returns>New filter expression</returns>
    public static FilterExpression Initialize<TDestintaion>(string filter, IConfigurationProvider provider)
        where TDestintaion : class, IBaseDto
    {
        FilterExpression filterExpression = new FilterExpression();
        string separator;
        if (filter.Contains("!:"))
        {
            filterExpression.ExpressionType = FilterExpressionType.Exclude;
            separator = "!:";
        }
        else if (filter.Contains(':'))
        {
            filterExpression.ExpressionType = FilterExpressionType.Include;
            separator = ":";
        }
        else
        {
            filterExpression.ExpressionType = FilterExpressionType.Undefined;
            return filterExpression;
        }
        string filterPath = filter[..filter.IndexOf(separator)];
        string[] segments = filterPath.Split('.');
        filterExpression.Key = segments[0].ToPascalCase();
        filterExpression.EndPoint = EntityFrameworkFiltersExtension.GetExpressionEndpoint(filterExpression.Key, provider, typeof(TDestintaion), out Type propertyType);
        filterExpression.Value = filter[(filter.IndexOf(separator) + separator.Length)..];
        filterExpression.EntityType = DtoExtension.GetDtoOriginType(typeof(TDestintaion));

        if (segments.Length > 1)
        {
            filterExpression.InnerFilterExpression = InvokeInitialize(
                string.Format("{0}{1}{2}",
                    string.Join('.', segments[1..]),
                    separator,
                    filterExpression.Value),
                provider,
                propertyType);
        }

        return filterExpression;
    }

    /// <summary>
    /// Invokes Initialize method with spectified property type.
    /// </summary>
    private static FilterExpression InvokeInitialize(string filter, IConfigurationProvider provider, Type propertyType)
    {
        var methodInfo = typeof(FilterExpression).GetMethod(nameof(Initialize));
        var genericMethod = methodInfo.MakeGenericMethod(propertyType);
        object[] parameters = [filter, provider];
        return (FilterExpression)genericMethod.Invoke(null, parameters);
    }
}

public enum FilterExpressionType
{
    Include, Exclude, Undefined
}
