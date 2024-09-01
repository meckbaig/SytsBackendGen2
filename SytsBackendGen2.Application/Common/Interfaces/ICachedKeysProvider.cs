using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Common.Interfaces;

/// <summary>
/// Interface to track entities and invalidate cache if tracked entities was changed.
/// </summary>
public interface ICachedKeysProvider
{
    /// <summary>
    /// Adds entity key to a tracking.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="expires">Expitation time.</param>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="id">Id of the entity.</param>
    /// <returns><see langword="true" /> if the addition was successful; otherwise, <see langword="false" />.</returns>
    Task<bool> TryAddKeyToIdIfNotPresentAsync(string key, DateTimeOffset expires, Type entityType, int id);

    /// <summary>
    /// Saves ids by a key.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <returns><see langword="true" /> if the completion was successful; otherwise, <see langword="false" />.</returns>
    Task<bool> TryCompleteFormationAsync(string key);

    /// <summary>
    /// Gets and removes all keys, containing specified entity id.
    /// </summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="id">Id of the entity.</param>
    /// <returns>Keys, containing specified entity id.</returns>
    Task<List<string>> GetAndRemoveKeysByIdAsync(Type entityType, int id);
}
