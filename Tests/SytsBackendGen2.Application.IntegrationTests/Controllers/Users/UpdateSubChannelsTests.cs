using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Users;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Services.Authorization;
using Docker.DotNet.Models;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Users;

public class UpdateSubChannelsTests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

    public UpdateSubChannelsTests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task UpdateSubChannels_ReturnSuccess_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var updateRequest = new UpdateSubChannelsCommand
        {
            channels = new List<SubChannelDto>
            {
                new SubChannelDto
                {
                    Title = "Test Channel",
                    ThumbnailUrl = "https://example.com/thumbnail.jpg",
                    ChannelId = "UC1234567890"
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("api/v1/Users/UpdateSubChannels", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<UpdateSubChannelsResponse>();

        Assert.NotNull(content);
        Assert.NotEqual((int)response.StatusCode, 500);
        Assert.NotEqual(default, content.LastChannelsUpdate);

        var authResponse = await AuthorizeUser("valid_access_token");
        for (int i = 0; i < authResponse.UserData.SubChannels.Count; i++)
        {
            Assert.Equal(authResponse.UserData.SubChannels[i].Title, updateRequest.channels[i].Title);
            Assert.Equal(authResponse.UserData.SubChannels[i].ThumbnailUrl, updateRequest.channels[i].ThumbnailUrl);
            Assert.Equal(authResponse.UserData.SubChannels[i].ChannelId, updateRequest.channels[i].ChannelId);
        }
    }

    [Fact]
    public async Task UpdateSubChannels_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        var updateRequest = new UpdateSubChannelsCommand
        {
            channels = new List<SubChannelDto>
            {
                new SubChannelDto
                {
                    Title = "Test Channel",
                    ThumbnailUrl = "https://example.com/thumbnail.jpg",
                    ChannelId = "UC1234567890"
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("api/v1/Users/UpdateSubChannels", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    private async Task<AuthorizeUserResponse> AuthorizeUser(string accessToken)
    {
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();
    }
}
