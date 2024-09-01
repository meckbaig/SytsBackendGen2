using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities.Authentification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.DTOs.Users;

public record UserEditDto : IEditDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }

    public static Type GetOriginType()
    {
        return typeof(User);
    }

    public static Type GetValidatorType()
    {
        throw new NotImplementedException();
    }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserEditDto>().ReverseMap();
            //CreateMap<UserEditDto, User>()
            //    .ForMember(dest => dest.RoleId, opt => opt.PreCondition(src => src.RoleId > 0))
            //    .ForAllMembers(opts =>
            //    {
            //        opts.Condition((src, dest, srcMember) 
            //            => srcMember != null);
            //    });
        }
    }
}
