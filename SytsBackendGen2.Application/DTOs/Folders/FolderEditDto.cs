using AutoMapper;
using FluentValidation;
using Newtonsoft.Json.Linq;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.DTOs.Folders;

public record FolderEditDto : IEditDto
{
    //public Guid Guid { get; set; }
    public string Name { get; set; }
    //public DateTime? LastChannelsUpdate { get; set; }
    //public DateTime? LastVideosAccess { get; set; }
    public JArray SubChannels { get; set; }
    //public int ChannelsCount { get; set; } = 0;
    public string? Color { get; set; } = "#ffffff";
    public string? Icon { get; set; }
    public JArray YoutubeFolders { get; set; }
    public AccessDto Access { get; set; }

    public static Type GetOriginType() => typeof(Folder);
    public static Type GetValidatorType() => typeof(Validator);

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FolderEditDto, Folder>()
                .ForMember(m => m.Access, opt => opt.MapFrom(f => f.Access))
                .ForMember(m => m.SubChannelsJson, opt => opt.MapFrom(f => f.SubChannels.ToString()))
                .ForMember(m => m.YoutubeFolders, opt => opt.MapFrom(f => f.YoutubeFolders.ToString()));
        }
    }
    internal class Validator : AbstractValidator<FolderEditDto>
    {
        public Validator(IAppDbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.SubChannels)
                .NotNull();
            RuleFor(x => x.Color)
                .NotEmpty()
                .Matches(@"^#[a-fA-F0-9]{6}$");
            RuleFor(x => x.YoutubeFolders)
                .NotNull();
            RuleFor(x => x.Access)
                .Must((q, p) => BeValidAccess(p, context));
        }

        private bool BeValidAccess(AccessDto access, IAppDbContext context)
        {
            if (access == null) 
                return false;
            return context.Access.Any(a => a.Id == access.Id && a.Name == access.Name);
        }
    }
}