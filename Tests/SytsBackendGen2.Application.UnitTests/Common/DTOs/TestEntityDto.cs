using AutoMapper;
using SytsBackendGen2.Application.Common.Attributes;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.UnitTests.Common.Entities;

namespace SytsBackendGen2.Application.UnitTests.Common.DTOs;

public record TestEntityDto : IBaseDto
{
    [Filterable(CompareMethod.Equals)]
    public int Id { get; set; }

    public string EntityName { get; set; }

    public string OriginalDescription { get; set; }

    public string ReverseDescription { get; set; }

    public string DateString { get; set; }

    [Filterable(CompareMethod.Equals)]
    public int SomeCount { get; set; }

    [Filterable(CompareMethod.Nested)]
    public List<TestNestedEntityDto> NestedThings { get; set; }

    [Filterable(CompareMethod.ById)]
    public TestNestedEntityDto SomeInnerEntity { get; set; }

    public static Type GetOriginType()
    {
        return typeof(TestEntity);
    }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TestEntity, TestEntityDto>()
                .ForMember(m => m.EntityName, opt => opt.MapFrom(o => o.Name))
                .ForMember(m => m.OriginalDescription, opt => opt.MapFrom(o => o.Description))
                .ForMember(m => m.ReverseDescription, opt => opt.MapFrom(o => string.Concat(o.Description.ToCharArray().Reverse())))
                .ForMember(m => m.DateString, opt => opt.MapFrom(o => o.Date.ToLongDateString()))
                .ForMember(m => m.SomeCount, opt => opt.MapFrom(o => o.SomeCount))
                .ForMember(m => m.NestedThings, opt => opt.MapFrom(o => o.TestNestedEntities))
                .ForMember(m => m.SomeInnerEntity, opt => opt.MapFrom(o => o.InnerEntity));
        }
    }
}