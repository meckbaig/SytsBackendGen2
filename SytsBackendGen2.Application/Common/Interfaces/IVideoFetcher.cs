using Newtonsoft.Json.Linq;
using SytsBackendGen2.Application.DTOs.Folders;

namespace SytsBackendGen2.Application.Common.Interfaces;

/// <summary>
/// Provides methods for fetching and processing YouTube videos.
/// </summary>
public interface IVideoFetcher
{
    /// <summary>
    /// Fetches videos from YouTube for the given subscribed channels.
    /// </summary>
    /// <param name="subChannels">A list of subscribed channels to fetch videos from.</param>
    /// <param name="youtubeFolders">An array of YouTube folder names to fetch videos from (e.g., "videos", "streams").</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a boolean indicating whether the fetch operation was successful.</returns>
    public Task<bool> Fetch(List<SubChannelDto> subChannels, string[] youtubeFolders);

    /// <summary>
    /// Retrieves the list of processed videos, optionally marking new videos since the last fetch.
    /// </summary>
    /// <param name="lastVideoId">The ID of the last video from the previous fetch. Used to mark new videos. If empty, no videos will be marked as new.</param>
    /// <returns>A list of dynamic objects representing the fetched videos, sorted by date.</returns>
    public List<dynamic> ToList(string lastVideoId = "");
}
