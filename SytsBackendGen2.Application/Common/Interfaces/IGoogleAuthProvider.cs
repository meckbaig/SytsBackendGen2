using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.DTOs.Users;

namespace SytsBackendGen2.Application.Common.Interfaces;

/// <summary>
/// Provides authentication and data retrieval methods for Google services.
/// </summary>
public interface IGoogleAuthProvider
{
    /// <summary>
    /// Authorizes a user using their Google access token.
    /// </summary>
    /// <param name="accessToken">The Google access token for the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user preview data if successful, or null if unsuccessful.</returns>
    Task<UserPreviewDto?> AuthorizeAsync(string accessToken);

    /// <summary>
    /// Retrieves the YouTube channel ID for a given username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the YouTube channel ID as a string.</returns>
    Task<string> GetYoutubeIdByName(string username);

    /// <summary>
    /// Retrieves a list of subscribed channels for a given YouTube channel ID.
    /// </summary>
    /// <param name="channelId">The YouTube channel ID to retrieve subscriptions for.</param>
    /// <param name="nextPageToken">An optional token for retrieving the next page of results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with a list of subscribed channels and the next page token (if available).</returns>
    Task<(List<SubChannelDto>, string?)> GetSubChannels(string channelId, string? nextPageToken = null);
}
