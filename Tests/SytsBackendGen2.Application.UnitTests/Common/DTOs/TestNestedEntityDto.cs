using AutoMapper;
using SytsBackendGen2.Application.Common.Attributes;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.UnitTests.Common.Entities;

namespace SytsBackendGen2.Application.UnitTests.Common.DTOs;

public record TestNestedEntityDto : IBaseDto
{
    [Filterable(CompareMethod.Equals)]
    public int Id { get; set; }

    public string NestedName { get; set; }

    [Filterable(CompareMethod.Equals)]
    public int Number { get; set; }

    public static Type GetOriginType()
    {
        return typeof(TestNestedEntity);
    }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TestNestedEntity, TestNestedEntityDto>()
                .ForMember(m => m.Id, opt => opt.MapFrom(o => o.Id))
                .ForMember(m => m.NestedName, opt => opt.MapFrom(o => o.Name))
                .ForMember(m => m.Number, opt => opt.MapFrom(o => (int)Math.Round(o.Number)));
        }
    }
}