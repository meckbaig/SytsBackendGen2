using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Services.Authorization;
using SytsBackendGen2.Application.DTOs.Users;
using SytsBackendGen2.Application.Services.Folders;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Authorization;

public class RefreshTokenTests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

    public RefreshTokenTests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RefreshToken_ReturnSuccess_WithValidRefreshToken()
    {
        // Arrange
        var initialResponse = await AuthorizeUser("valid_access_token");
        var refreshTokenCommand = new RefreshTokenCommand { refreshToken = initialResponse.RefreshToken };

        // Act
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialResponse.Token);
        var response = await _client.PostAsJsonAsync("api/v1/Authorization/RefreshToken", refreshTokenCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.NotEmpty(content.RefreshToken);
        Assert.NotEqual(initialResponse.Token, content.Token);
        Assert.NotEqual(initialResponse.RefreshToken, content.RefreshToken);

        // Verify if the new token is valid
        await VerifyTokenIsValid(content.Token);
    }

    [Fact]
    public async Task RefreshToken_ReturnBadRequest_WithInvalidRefreshToken()
    {
        // Arrange
        var initialResponse = await AuthorizeUser("valid_access_token");
        var refreshTokenCommand = new RefreshTokenCommand { refreshToken = "invalid_refresh_token" };

        // Act
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialResponse.Token);
        var response = await _client.PostAsJsonAsync("api/v1/Authorization/RefreshToken", refreshTokenCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("RefreshTokenNotValid", content);
    }

    [Fact]
    public async Task RefreshToken_ReturnUnauthorized_WithoutToken()
    {
        // Arrange
        var refreshTokenCommand = new RefreshTokenCommand { refreshToken = "some_refresh_token" };

        // Act
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("api/v1/Authorization/RefreshToken", refreshTokenCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ReturnBadRequest_WithExpiredRefreshToken()
    {
        // Arrange
        var initialResponse = await AuthorizeUser("valid_access_token");
        var refreshTokenCommand = new RefreshTokenCommand { refreshToken = TestAuth.MainUserExpiredRefreshToken };

        // Act
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialResponse.Token);
        var response = await _client.PostAsJsonAsync("api/v1/Authorization/RefreshToken", refreshTokenCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("RefreshTokenExpired", content);
    }

    private async Task<AuthorizeUserResponse> AuthorizeUser(string accessToken)
    {
        var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();
    }

    private async Task VerifyTokenIsValid(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsync("api/v1/Folders", JsonContent.Create(new CreateFolderCommand { name = "Test Folder" }));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateFolderResponse>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content.Folder.Guid);
        Assert.Equal("Test Folder", content.Folder.Name);
    }
}
