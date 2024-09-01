using AutoMapper;
using FluentValidation;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.UnitTests.Common.Entities;

namespace SytsBackendGen2.Application.UnitTests.Common.DTOs;

public record TestEditDtoWithLongNameMapping : IEditDto
{
    public int NestedId { get; set; }
    public string NestedName { get; set; }

    public static Type GetOriginType()
    {
        return typeof(TestEntity);
    }

    public static Type GetValidatorType()
    {
        return typeof(Validator);
    }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TestEditDtoWithLongNameMapping, TestEntity>()
                .ForMember(m => m.InnerEntityId, opt => opt.MapFrom(o => o.NestedId))
                .ForPath(m => m.InnerEntity.Name, opt => opt.MapFrom(o => o.NestedName));
        }
    }

    internal class Validator : AbstractValidator<TestEditDtoWithLongNameMapping>
    {
        public Validator()
        {
            RuleFor(x => x.NestedName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);
        }
    }

}
