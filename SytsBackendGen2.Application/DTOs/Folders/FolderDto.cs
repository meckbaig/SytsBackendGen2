using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Folders;

public record FolderDto : IBaseDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public DateTimeOffset? LastVideosAccess { get; set; }
    public List<SubChannelDto> SubChannels { get; set; }
    public int ChannelsCount { get; set; } = 0;
    public string? Color { get; set; } = "#ffffff";
    public string? Icon { get; set; }
    public string[] YoutubeFolders { get; set; }
    public AccessDto Access { get; set; }

    public static Type GetOriginType()
    {
        return typeof(Folder);
    }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Folder, FolderDto>()
                .ForMember(m => m.Access, opt => opt.MapFrom(f => f.Access))
                .ForMember(m => m.SubChannels, opt => opt.MapFrom(f => ConvertJsonToExpandoList(f.SubChannelsJson)))
                .ForMember(m => m.YoutubeFolders, opt => opt.MapFrom(f => JsonConvert.DeserializeObject<string[]>(f.YoutubeFolders)))
                .ForMember(m => m.LastModified, opt => opt.MapFrom(f => f.LastModified))
                .ForMember(m => m.LastVideosAccess, opt => opt.MapFrom(f => f.GetLastVideosCall()))
                ;
        }

        private static List<ExpandoObject> ConvertJsonToExpandoList(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<ExpandoObject>();
            }

            JArray jsonArray = JArray.Parse(jsonString);

            List<ExpandoObject> expandoList = new List<ExpandoObject>();

            foreach (JObject jsonObject in jsonArray)
            {
                ExpandoObject expandoObject = jsonObject.ToObject<ExpandoObject>();
                expandoList.Add(expandoObject);
            }

            return expandoList;
        }
    }
}