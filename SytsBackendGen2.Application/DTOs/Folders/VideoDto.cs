using AutoMapper;
using System.Dynamic;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Folders;

public class VideoDto : IBaseDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string SimpleLength { get; set; }
    public string ViewCount { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public string ChannelId { get; set; }
    public string ChannelTitle { get; set; }
    public string ChannelThumbnail { get; set; }
    public int MaxThumbnail { get; set; }
    public bool IsNew { get; set; }
    public LiveStreamingDetailsDto? LiveStreamingDetails { get; set; }

    public static Type GetOriginType()
    {
        return typeof(ExpandoObject);
    }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExpandoObject, VideoDto>().ConvertUsing(expandoObject => MapExpandoToVideoDto(expandoObject));
        }

        private VideoDto MapExpandoToVideoDto(ExpandoObject expandoObject)
        {
            var expandoDict = expandoObject as IDictionary<string, object>;
            var videoDto = new VideoDto
            {
                Id = GetValueOrDefault(expandoDict, "id", string.Empty),
                Title = GetValueOrDefault(expandoDict, "title", string.Empty),
                SimpleLength = GetValueOrDefault(expandoDict, "simpleLength", string.Empty),
                ViewCount = GetValueOrDefault(expandoDict, "viewCount", string.Empty),
                PublishedAt = GetValueOrDefault(expandoDict, "publishedAt", DateTimeOffset.MinValue),
                ChannelId = GetValueOrDefault(expandoDict, "channelId", string.Empty),
                ChannelTitle = GetValueOrDefault(expandoDict, "channelTitle", string.Empty),
                ChannelThumbnail = GetValueOrDefault(expandoDict, "channelThumbnail", string.Empty),
                MaxThumbnail = GetValueOrDefault(expandoDict, "maxThumbnail", 0),
                IsNew = GetValueOrDefault(expandoDict, "isNew", false),
                LiveStreamingDetails = GetLiveStreamingDetails(expandoDict, "liveStreamingDetails")
            };
            return videoDto;
        }

        private T GetValueOrDefault<T>(IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is T result)
                    return result;
                if (value == null)
                    return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return defaultValue;
        }

        private LiveStreamingDetailsDto? GetLiveStreamingDetails(IDictionary<string, object> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key))
                return null;

            var liveStreamingDetails = dictionary[key] as IDictionary<string, object>;
            if (!liveStreamingDetails.TryGetValue("scheduledStartTime", out var _)) 
                return null;
            return new LiveStreamingDetailsDto
            {
                ScheduledStartTime = GetValueOrDefault(liveStreamingDetails, "scheduledStartTime", DateTimeOffset.MinValue),
                ActualStartTime = GetValueOrDefault(liveStreamingDetails, "actualStartTime", (DateTimeOffset?)null),
                ConcurrentViewers = GetValueOrDefault(liveStreamingDetails, "concurrentViewers", (int?)null)
            };
        }
    }
}

public class LiveStreamingDetailsDto
{
    public DateTimeOffset ScheduledStartTime { get; set; }
    public DateTimeOffset? ActualStartTime { get; set; }
    public int? ConcurrentViewers { get; set; }
}