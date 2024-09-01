using AutoMapper;
using FluentValidation;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.UnitTests.Common.Entities;
using static SytsBackendGen2.Application.UnitTests.Common.ValidationTestsEntites;

namespace SytsBackendGen2.Application.UnitTests.Common.DTOs;

public record TestEntityEditDto : IEditDto
{
    public int Id { set; get; }
    public string EntityName { get; set; }
    public string OriginalDescription { get; set; }
    public string DateString { get; set; }
    public List<TestNestedEntityEditDto> NestedThings { get; set; }
    public TestNestedEntityEditDto SomeInnerEntity { get; set; }
    public int SomeInnerEntityId { get; set; }

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
            CreateMap<TestEntityEditDto, TestEntity>()
                .ForMember(m => m.Name, opt => opt.MapFrom(o => o.EntityName))
                .ForMember(m => m.Description, opt => opt.MapFrom(o => o.OriginalDescription))
                .ForMember(m => m.Date, opt => opt.MapFrom(o => ConvertToDateOnly(o.DateString)))
                .ForMember(m => m.TestNestedEntities, opt => opt.MapFrom(o => o.NestedThings))
                .ForMember(m => m.InnerEntity, opt => opt.MapFrom(o => o.SomeInnerEntity))
                .ForMember(m => m.InnerEntityId, opt => opt.MapFrom(o => o.SomeInnerEntityId));
        }

        private DateOnly ConvertToDateOnly(string dateString)
        {
            //CultureInfo provider = CultureInfo.GetCultureInfo("ru-RU");
            DateTime dateTime = DateTime.Parse(dateString);
            DateOnly dateOnly = DateOnly.FromDateTime(dateTime);
            return dateOnly;
        }
    }

    internal class Validator : AbstractValidator<TestEntityEditDto>
    {
        public Validator()
        {
            RuleFor(x => x.EntityName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);
            RuleFor(x => x.OriginalDescription)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);
            RuleFor(x => x.DateString)
                .NotEmpty()
                .Must(BeValidDateString);
            RuleFor(x => x.SomeInnerEntity)
                .SetValidator(new TestNestedEntityEditDto.Validator());
            RuleForEach(x => x.NestedThings)
                .SetValidator(new TestNestedEntityEditDto.Validator());
        }

        private bool BeValidDateString(string dateString)
        {
            return DateTime.TryParse(dateString, out var _);
        }
    }
}