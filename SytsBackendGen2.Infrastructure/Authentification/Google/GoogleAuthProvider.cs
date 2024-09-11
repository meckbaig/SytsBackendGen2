using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
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

        UserPreviewDto userDto = new(userInfo.name.ToString(), userInfo.email.ToString(), userInfo.picture.ToString());
        //userDto.YoutubeId = await GetYoutubeIdByName(userDto.Name);

        return userDto;
    }

    public async Task<string> GetYoutubeIdByName(string username)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        var response = await _httpClient.GetAsync(
            $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={username}" +
            $"&type=channel&key={_youtubeKey}");
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadAsStringAsync();
        var channelsArray = JsonConvert.DeserializeObject<dynamic>(responseData);

        return channelsArray.items[0].id.channelId;
    }

    public async Task<(List<SubChannelDto>, int, string?)> GetSubChannels(string channelId, string? nextPageToken = null)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        var url = new UriBuilder("https://youtube.googleapis.com/youtube/v3/subscriptions")
        {
            Query = BuildGetSubChannelsQueryString(channelId, nextPageToken)
        };
        var response = await _httpClient.GetAsync(url.ToString());
        response.EnsureSuccessStatusCode(); 

        var responseDataString = await response.Content.ReadAsStringAsync();
        var responseData = JsonConvert.DeserializeObject<dynamic>(responseDataString);
        List<SubChannelDto> subChannelsResponse = new List<SubChannelDto>();

        foreach (var item in responseData.items)
        {
            subChannelsResponse.Add(new SubChannelDto 
            {
                Title = item.snippet.title, 
                ChannelId = item.snippet.resourceId.channelId,
                ThumbnailUrl = item.snippet.thumbnails["default"].url
            });
        }
        int totalResults = int.Parse(responseData["pageInfo"]["totalResults"].ToString());
        nextPageToken = responseData["nextPageToken"] ?? null;

        return (subChannelsResponse, totalResults, nextPageToken);
    }

    private string BuildGetSubChannelsQueryString(string channelId, string? nextPageToken = null)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["part"] = "snippet";
        query["channelId"] = channelId;
        query["maxResults"] = "50";
        query["prettyPrint"] = "false";
        query["key"] = _youtubeKey;
        if (!string.IsNullOrEmpty(nextPageToken))
        {
            query["pageToken"] = nextPageToken;
        }
        return query.ToString();
    }

}
