using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Users;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Services.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SytsBackendGen2.Application.Common.Interfaces;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Users;

public class UpdateSubChannelsV1_1Tests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

    public UpdateSubChannelsV1_1Tests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task UpdateSubChannelsV1_1_ReturnSuccess_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.PutAsync("api/v1.1/Users/UpdateSubChannels", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<UpdateSubChannelsV1_1Response>();

        Assert.NotNull(content);
        Assert.NotEqual((int)response.StatusCode, 500);
        Assert.NotEqual(default, content.LastChannelsUpdate);
        Assert.NotNull(content.SubChannels);
        Assert.Single(content.SubChannels);

        var subChannel = content.SubChannels[0];
        Assert.Equal("Test Channel", subChannel.Title);
        Assert.Equal("https://example.com/thumbnail.jpg", subChannel.ThumbnailUrl);
        Assert.Equal("UC1234567890", subChannel.ChannelId);

        // Verify that the subChannels were actually updated
        var authResponse = await AuthorizeUser("valid_access_token");

        Assert.NotNull(authResponse.UserData.SubChannels);
        Assert.Single(authResponse.UserData.SubChannels);
        Assert.Equal(subChannel.Title, authResponse.UserData.SubChannels[0].Title);
        Assert.Equal(subChannel.ThumbnailUrl, authResponse.UserData.SubChannels[0].ThumbnailUrl);
        Assert.Equal(subChannel.ChannelId, authResponse.UserData.SubChannels[0].ChannelId);
    }

    [Fact]
    public async Task UpdateSubChannelsV1_1_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.PutAsync("api/v1.1/Users/UpdateSubChannels", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSubChannelsV1_1_ReturnCorrectData_WhenMultiplePages()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Modify the MockGoogleAuthProvider to return multiple pages
        var mockProvider = _factory.Services.GetRequiredService<IGoogleAuthProvider>() as MockGoogleAuthProvider;
        mockProvider.SetMultiplePages(true);

        // Act
        var response = await _client.PutAsync("api/v1.1/Users/UpdateSubChannels", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<UpdateSubChannelsV1_1Response>();

        Assert.NotNull(content);
        Assert.NotEqual((int)response.StatusCode, 500);
        Assert.NotEqual(default, content.LastChannelsUpdate);
        Assert.NotNull(content.SubChannels);
        Assert.Equal(3, content.SubChannels.Count); // Assuming 3 total channels across all pages

        // Verify that all channels from multiple pages are present
        Assert.Contains(content.SubChannels, c => c.Title == "Test Channel 1");
        Assert.Contains(content.SubChannels, c => c.Title == "Test Channel 2");
        Assert.Contains(content.SubChannels, c => c.Title == "Test Channel 3");

        // Reset the MockGoogleAuthProvider
        mockProvider.SetMultiplePages(false);
    }
    private async Task<AuthorizeUserResponse> AuthorizeUser(string accessToken)
    {
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();
    }
}
