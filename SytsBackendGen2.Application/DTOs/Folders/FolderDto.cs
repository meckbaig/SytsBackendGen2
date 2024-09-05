using AutoMapper;
using Newtonsoft.Json.Linq;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Users;

public record FolderDto : IBaseDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public DateTime? LastChannelsUpdate { get; set; }
    public string? SubChannels { get; set; }
    public int ChannelsCount { get; set; } = 0;
    public string? Color { get; set; } = "#ffffff";
    public string? Icon { get; set; }
    public string? YoutubeFolders { get; set; }
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
                .ForMember(m => m.Access, opt => opt.MapFrom(f => f.Access));
        }
    }
}