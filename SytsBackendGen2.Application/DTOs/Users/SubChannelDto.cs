using AutoMapper;
using System.Dynamic;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Folders;

public class SubChannelDto : IBaseDto
{
    public string Title { get; set; }
    public string ThumbnailUrl { get; set; }
    public string ChannelId { get; set; }

    public static Type GetOriginType()
    {
        return typeof(ExpandoObject);
    }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExpandoObject, SubChannelDto>().ConvertUsing(expandoObject => MapExpandoToSubChannelDto(expandoObject));
        }

        private SubChannelDto MapExpandoToSubChannelDto(ExpandoObject expandoObject)
        {
            var expandoDict = expandoObject as IDictionary<string, object>;
            var subChannelDto = new SubChannelDto
            {
                Title = GetValueOrDefault(expandoDict, "title", string.Empty),
                ThumbnailUrl = GetValueOrDefault(expandoDict, "thumbnailUrl", string.Empty),
                ChannelId = GetValueOrDefault(expandoDict, "channelId", string.Empty)
            };
            return subChannelDto;
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
