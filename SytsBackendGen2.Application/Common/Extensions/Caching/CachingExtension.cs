using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SytsBackendGen2.Application.Common.Extensions.Caching;

public static class CachingExtension
{
    /// <summary>
    /// Represents a cache response with data of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the data in the cache response.</typeparam>
    public sealed record CacheResponse<TResult>
    {
        /// <summary>
        /// The data from the cache response.
        /// </summary>
        public TResult Data { get; init; }

        /// <summary>
        /// A value indicating whether the data was fetched from the cache.
        /// </summary>
        public bool FetchedFromCache { get; init; }
    }

    private static readonly DistributedCacheEntryOptions CacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600)
    };

    /// <summary>
    /// Gets data from cache if present; otherwise, executes factory and saves in cache.
    /// </summary>
    /// <typeparam name="TRequestResult">Type of data from request expression.</typeparam>
    /// <typeparam name="TDto">Type of data to project result data to.</typeparam>
    /// <param name="cache">Caching provider.</param>
    /// <param name="key">Key for saving data.</param>
    /// <param name="requestResultFactory">Execution factory with operation to get data.</param>
    /// <param name="projectionFactory">Execution factory to project data to <typeparamref name="TDto"/></param>
    /// <param name="cancellationToken"></param>
    /// <param name="options">Options for caching provider.</param>
    /// <returns></returns>
    public static async Task<CacheResponse<TDto>> GetOrCreateAsync<TRequestResult, TDto>(
        this IDistributedCache cache,
        string key,
        Func<Task<TRequestResult>> requestResultFactory,
        Func<TRequestResult, TDto> projectionFactory,
        CancellationToken cancellationToken = default,
        bool forceRefresh = false,
        DistributedCacheEntryOptions? options = null)
    {
        if (!forceRefresh)
        {
            string cachedMember = await cache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(cachedMember))
            {
                return new CacheResponse<TDto>
                {
                    Data = JsonConvert.DeserializeObject<TDto>(cachedMember),
                    FetchedFromCache = true
                };
            }
        }

        TRequestResult requestResult = await requestResultFactory.Invoke();
        options ??= CacheEntryOptions;

        TDto dtoResult = projectionFactory.Invoke(requestResult);
        await cache.SetStringAsync(key,
            JsonConvert.SerializeObject(dtoResult),
            options,
            cancellationToken);

        return new CacheResponse<TDto>
        {
            Data = dtoResult,
            FetchedFromCache = false
        }; 
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
    public static async Task<CacheResponse<TDto>> GetOrCreateAsync<TRequestResult, TDto>(
        this IDistributedCache cache,
        string key,
        Func<Task<TRequestResult>> requestResultFactory,
        Func<TRequestResult, TDto> projectionFactory,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default,
        bool forceRefresh = false)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        };
        return await cache.GetOrCreateAsync(
            key,
            requestResultFactory,
            projectionFactory,
            cancellationToken,
            forceRefresh,
            options);
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