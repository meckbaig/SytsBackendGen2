using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Services.Authorization;
using SytsBackendGen2.Application.DTOs.Users;
using SytsBackendGen2.Application.Services.Folders;
using System.Net.Http.Headers;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Authorization;

public class AuthorizeUserTests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

    public AuthorizeUserTests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Authorize_ReturnSuccess_WithValidAccessToken()
    {
        // Arrange
        var accessToken = "valid_access_token";

        // Act
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();

        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.NotEmpty(content.RefreshToken);
        Assert.NotNull(content.UserData);
        Assert.Equal("Test User", content.UserData.Name);
        Assert.Equal("first@gmail.com", content.UserData.Email);
        Assert.Equal("https://example.com/profile.jpg", content.UserData.Picture);
        Assert.Equal("UCOByaobXOl6YYxUNBxSwe_7", content.UserData.YoutubeId);

        // Verify if the token is valid by making a request to a protected endpoint
        await VerifyTokenIsValid(content.Token);
    }

    [Fact]
    public async Task Authorize_ReturnBadRequest_WithInvalidAccessToken()
    {
        // Arrange
        var accessToken = "invalid_access_token";

        // Modify the MockGoogleAuthProvider to throw an exception for invalid token
        var mockProvider = _factory.Services.GetRequiredService<IGoogleAuthProvider>() as MockGoogleAuthProvider;
        mockProvider.SetInvalidAccessToken(accessToken);

        // Act
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("AccessTokenNotValid", content);

        // Reset the MockGoogleAuthProvider
        mockProvider.SetInvalidAccessToken(null);
    }

    [Fact]
    public async Task Authorize_CreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var accessToken = "new_user_access_token";
        var newUserEmail = "newuser@example.com";

        var mockProvider = _factory.Services.GetRequiredService<IGoogleAuthProvider>() as MockGoogleAuthProvider;
        mockProvider.SetNewUser(newUserEmail);

        // Act
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();

        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.NotEmpty(content.RefreshToken);
        Assert.NotNull(content.UserData);
        Assert.Equal(newUserEmail, content.UserData.Email);

        await VerifyTokenIsValid(content.Token);

        // Reset the MockGoogleAuthProvider
        mockProvider.SetNewUser(null);
    }

    private async Task VerifyTokenIsValid(string token)
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("api/v1/Folders", JsonContent.Create(new CreateFolderCommand { name = "Test Folder" }));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateFolderResponse>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content.Folder.Guid);
        Assert.Equal("Test Folder", content.Folder.Name);
    }
}
