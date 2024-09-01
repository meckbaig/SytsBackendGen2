using AutoMapper;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities.Authentification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.DTOs.Roles;

public record RolePreviewDto : IBaseDto
{
    public string Name { get; set; }
    public string[] PermissionsNames { get; set; }

    public static Type GetOriginType()
    {
        return typeof(Role);
    }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Role, RolePreviewDto>()
                .ForMember(r => r.Name, opt => opt.MapFrom(r => r.Name))
                .ForMember(r => r.PermissionsNames, opt => opt.MapFrom(r => r.Permissions.Select(p => p.Name)));
        }
    }
}
