using AutoMapper.Internal;
using Microsoft.Extensions.Caching.Distributed;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Common;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace SytsBackendGen2.Application.Common.Extensions.Caching;

public static class CachingExtension
{
    private static readonly DistributedCacheEntryOptions CacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Gets data from cache if present; otherwise, executes factory and saves in cache.
    /// </summary>
    /// <typeparam name="TRequestResult">Type of data from request expression.</typeparam>
    /// <typeparam name="TDto">Type of data to project result data to.</typeparam>
    /// <param name="cache">Caching provider.</param>
    /// <param name="cachedKeysProvider">Provider to perform saving of cached keys.</param>
    /// <param name="key">Key for saving data.</param>
    /// <param name="requestResultFactory">Execution factory with operation to get data.</param>
    /// <param name="projectionFactory">Execution factory to project data to <typeparamref name="TDto"/></param>
    /// <param name="cancellationToken"></param>
    /// <param name="options">Options for caching provider.</param>
    /// <returns></returns>
    public static async Task<TDto> GetOrCreateAsync<TRequestResult, TDto>(
        this IDistributedCache cache,
        ICachedKeysProvider cachedKeysProvider,
        string key,
        Func<Task<TRequestResult>> requestResultFactory,
        Func<TRequestResult, TDto> projectionFactory,
        CancellationToken cancellationToken = default,
        DistributedCacheEntryOptions? options = null)
    {
        string cachedMember = await cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(cachedMember))
            return JsonConvert.DeserializeObject<TDto>(cachedMember);

        TRequestResult requestResult = await requestResultFactory.Invoke();
        options ??= CacheEntryOptions;
        
        await TrackIds(cachedKeysProvider, requestResult, key,
            DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow ?? TimeSpan.Zero));

        TDto dtoResult = projectionFactory.Invoke(requestResult);
        await cache.SetStringAsync(key,
            JsonConvert.SerializeObject(dtoResult),
            options,
            cancellationToken);
        return dtoResult;
    }

    /// <summary>
    /// Gets data from cache if present; otherwise, executes factory and saves in cache.
    /// </summary>
    /// <typeparam name="TRequestResult">Type of data from request expression.</typeparam>
    /// <typeparam name="TDto">Type of data to project result data to.</typeparam>
    /// <param name="cache">Caching provider.</param>
    /// <param name="cachedKeysProvider">Provider to perform saving of cached keys.</param>
    /// <param name="key">Key for saving data.</param>
    /// <param name="requestResultFactory">Execution factory with operation to get data.</param>
    /// <param name="projectionFactory">Execution factory to project data to <typeparamref name="TDto"/></param>
    /// <param name="absoluteExpirationRelativeToNow">Caching lifetime.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TDto> GetOrCreateAsync<TRequestResult, TDto>(
        this IDistributedCache cache,
        ICachedKeysProvider cachedKeysProvider,
        string key,
        Func<Task<TRequestResult>> requestResultFactory,
        Func<TRequestResult, TDto> projectionFactory,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        };
        return await cache.GetOrCreateAsync(
            cachedKeysProvider,
            key,
            requestResultFactory,
            projectionFactory,
            cancellationToken,
            options);
    }

    /// <summary>
    /// Saves all id's from recieved data to invalidate cache when this data will be changed in the future
    /// </summary>
    /// <typeparam name="TResult">Type of data to track id's from.</typeparam>
    /// <param name="cachedKeysProvider">Provider to perform saving of cached keys.</param>
    /// <param name="result">Data to track id's from.</param>
    /// <param name="key">Key for saving data.</param>
    /// <param name="expires">Expiration time.</param>
    private static async Task TrackIds<TResult>(
        ICachedKeysProvider cachedKeysProvider,
        TResult result,
        string key,
        DateTimeOffset expires)
    {
        Type resultType = typeof(TResult);
        await TrackIdsInternal(cachedKeysProvider, result, resultType, key, expires);
        await cachedKeysProvider.TryCompleteFormationAsync(key);
    }

    /// <summary>
    /// Saves all id's from recieved data to invalidate cache when this data will be changed in the future
    /// </summary>
    /// <param name="cachedKeysProvider">Provider to perform saving of cached keys.</param>
    /// <param name="result">Data to track id's from.</param>
    /// <param name="resultType">Type of data to track id's from.</param>
    /// <param name="key">Key for saving data.</param>
    /// <param name="expires">Expiration time.</param>
    /// <remarks>DO NOT USE THIS METHOD OUTSIDE OF CALL IN TrackIds()</remarks>
    private static async Task TrackIdsInternal(
        ICachedKeysProvider cachedKeysProvider,
        object result,
        Type resultType,
        string key,
        DateTimeOffset expires)
    {
        if (resultType.IsCollection())
        {
            IEnumerable resultCollection = (IEnumerable)result;
            if (resultCollection == null)
                return;
            Type collectionElementType = resultType.GetGenericArguments().Single();
            foreach (var collectionElement in resultCollection)
            {
                await TrackIdsInternal(cachedKeysProvider, collectionElement, collectionElementType, key, expires);
            }
        }
        else if (typeof(BaseEntity).IsAssignableFrom(resultType))
        {
            var idProperty = resultType.GetProperty(nameof(BaseEntity.Id));
            int idValue = (int)idProperty.GetValue(result);
            if (await cachedKeysProvider.TryAddKeyToIdIfNotPresentAsync(key, expires, resultType, idValue))
            {
                foreach (var property in resultType.GetProperties())
                {
                    if (property.PropertyType.IsCollection() || typeof(BaseEntity).IsAssignableFrom(property.PropertyType))
                    {
                        var value = property.GetValue(result);
                        if (value != null)
                            await TrackIdsInternal(cachedKeysProvider, value, property.PropertyType, key, expires);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Removes data by key from cache.
    /// </summary>
    /// <param name="cache">Caching provider.</param>
    /// <param name="key">Key for removing data.</param>
    public static void RemoveFromCache(
        this IDistributedCache cache,
        string key)
    {
        cache.Remove(key);
    }
}