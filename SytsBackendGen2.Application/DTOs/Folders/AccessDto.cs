using AutoMapper;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Folders;

public record AccessDto : IBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; }

    public static Type GetOriginType()
    {
        return typeof(Access);
    }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Access, AccessDto>().ReverseMap();
        }
    }
}