using Microsoft.AspNetCore.JsonPatch.Adapters;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Extensions.DataBaseProvider;
using SytsBackendGen2.Domain.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.JsonPatch;

public class CustomAdapterFactory : IAdapterFactory
{
    internal static CustomAdapterFactory Default { get; } = new CustomAdapterFactory();

    public IAdapter Create(object target, IContractResolver contractResolver)
    {
        ArgumentNullException.ThrowIfNull(target, "target");
        ArgumentNullException.ThrowIfNull(contractResolver, "contractResolver");
        JsonContract jsonContract = contractResolver.ResolveContract(target.GetType());
        if (target is JObject)
        {
            return new JObjectAdapter();
        }

        if (target is IList)
        {
            return new CustomListAdapter();
        }

        if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
        {
            return (IAdapter)Activator
                .CreateInstance(typeof(DictionaryAdapter<,>)
                .MakeGenericType(
                    jsonDictionaryContract.DictionaryKeyType, 
                    jsonDictionaryContract.DictionaryValueType));
        }

        if (jsonContract is JsonDynamicContract)
        {
            return new DynamicObjectAdapter();
        }

        Type targetType = target.GetType();

        if (typeof(IEnumerable).IsAssignableFrom(targetType)/* && typeof(IDbContext).CanGetDbSet(targetType.GetGenericArguments()?[0])*/)
        {
            Type customDbSetAdapterType = typeof(CustomDbSetAdapter<>).MakeGenericType(targetType.GetGenericArguments()[0]);
            return (IAdapter)Activator.CreateInstance(customDbSetAdapterType);
        }

        return new PocoAdapter();
    }
}

internal static class AdapterError
{
    public static string FormatIndexOutOfBounds(object p0)
            => $"The index value provided by path segment '{p0}' is out of bounds of the array size.";

    public static string FormatInvalidIndexValue(object p0)
           => $"The path segment '{p0}' is invalid for an item Id.";

    public static string FormatInvalidValueForProperty(object p0)
           => $"The value '{p0}' is invalid for target location.";

    public static string FormatInvalidListType()
           => $"List items do not inherit interface {nameof(IEntityWithId)}";
}
