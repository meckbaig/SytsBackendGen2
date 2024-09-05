using AutoMapper;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.DTOs.Users;

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
            CreateMap<Access, AccessDto>();
        }
    }
}