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
    public string PublishedAt { get; set; }
    public string ChannelId { get; set; }
    public string ChannelTitle { get; set; }
    public string ChannelThumbnail { get; set; }
    public int MaxThumbnail { get; set; }
    public bool IsNew { get; set; }
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
                PublishedAt = GetValueOrDefault(expandoDict, "publishedAt", string.Empty),
                ChannelId = GetValueOrDefault(expandoDict, "channelId", string.Empty),
                ChannelTitle = GetValueOrDefault(expandoDict, "channelTitle", string.Empty),
                ChannelThumbnail = GetValueOrDefault(expandoDict, "channelThumbnail", string.Empty),
                MaxThumbnail = GetValueOrDefault(expandoDict, "maxThumbnail", 0),
                IsNew = GetValueOrDefault(expandoDict, "isNew", false)
            };
            return videoDto;
        }

        private T GetValueOrDefault<T>(IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is T result)
                    return result;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return defaultValue;
        }
    }
}
