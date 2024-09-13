using AutoMapper;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.DTOs.Users;

public record UserPreviewDto : IBaseDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Picture { get; set; }
    public string YoutubeId { get; set; }
    public List<SubChannelDto> SubChannels { get; set; }

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
                .ForMember(m => m.YoutubeId, opt => opt.MapFrom(u => u.YoutubeId))
                .ForMember(m => m.SubChannels, opt => opt.MapFrom(u => ConvertJsonToExpandoList(u.SubChannelsJson)));
        }

        private static List<ExpandoObject> ConvertJsonToExpandoList(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<ExpandoObject>();
            }

            JArray jsonArray = JArray.Parse(jsonString);

            List<ExpandoObject> expandoList = new List<ExpandoObject>();

            foreach (JObject jsonObject in jsonArray)
            {
                ExpandoObject expandoObject = jsonObject.ToObject<ExpandoObject>();
                expandoList.Add(expandoObject);
            }

            return expandoList;
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