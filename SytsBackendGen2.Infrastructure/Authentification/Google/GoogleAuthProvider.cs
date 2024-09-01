using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Users;

namespace SytsBackendGen2.Infrastructure.Authentification.Google;

public class GoogleAuthProvider : IGoogleAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _youtubeKey;

    public GoogleAuthProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _youtubeKey = configuration["YoutubeKey"];
    }

    public async Task<UserPreviewDto?> AuthorizeAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

        var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadAsStringAsync();
        var userInfo = JsonConvert.DeserializeObject<dynamic>(responseData);
        UserPreviewDto userDto = new(userInfo.name, userInfo.email, userInfo.picture);
        //userDto.YoutubeId = await GetYoutubeIdByName(userDto.Name);

        return userDto;
    }

    public async Task<string> GetYoutubeIdByName(string username)
    {
        var response = await _httpClient.GetAsync(
            $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={username}" +
            $"&type=channel&key={_youtubeKey}");
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadAsStringAsync();
        var channelsArray = JsonConvert.DeserializeObject<dynamic>(responseData);

        return channelsArray.items[0].id.channelId;
    }
}
