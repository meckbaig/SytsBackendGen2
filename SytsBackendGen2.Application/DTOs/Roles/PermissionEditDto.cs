using AutoMapper;
using FluentValidation;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities.Authentification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.DTOs.Roles;

public record PermissionEditDto : IEditDto
{
    public int Id { get; set; }

    public static implicit operator PermissionEditDto(int permId)
        => new() { Id = permId };
    //public static implicit operator PermissionEditDto(Int64 permId)
    //    => new() { Id = Convert.ToInt32(permId) };

    public static Type GetOriginType()
    {
        return typeof(Permission);
    }

    public static Type GetValidatorType()
    {
        return typeof(Validator);
    }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Permission, PermissionEditDto>().ReverseMap();
        }
    }

    internal class Validator : AbstractValidator<PermissionEditDto>
    {
        public Validator()
        {
            RuleFor(x => x.Id).MustBeExistingPermission();
        }
    }
}
