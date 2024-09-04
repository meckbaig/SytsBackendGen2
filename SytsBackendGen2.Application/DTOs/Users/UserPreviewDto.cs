using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.DTOs.Users;

public record UserPreviewDto : IBaseDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Picture { get; set; }
    public string YoutubeId { get; set; }

    public static Type GetOriginType()
    {
        return typeof(User);
    }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserPreviewDto>()
                .ForMember(m => m.Email, opt => opt.MapFrom(u => u.Email))
                .ForMember(m => m.Role, opt => opt.MapFrom(u => u.Role.Name))
                .ForMember(m => m.YoutubeId, opt => opt.MapFrom(u => u.YoutubeId));
        }
    }

    public UserPreviewDto() { }

    public UserPreviewDto(string name, string email, string picture)
    {
        Name = name;
        Email = email;
        Picture = picture;
    }
}