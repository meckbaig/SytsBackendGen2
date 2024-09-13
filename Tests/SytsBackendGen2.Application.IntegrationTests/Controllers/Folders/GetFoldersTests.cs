using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Folders;

public class GetFoldersTests
{
    private readonly HttpClient _client;
    public GetFoldersTests()
    {
        var app = new SytsBackendGen2WebApplicationFactory();
        _client = app.CreateClient();
    }

    [Fact]
    public async Task GetFolders_ReturnOnlyPublicFolders_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("api/v1/Folders");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFoldersResponse>();

        Assert.NotNull(content);
        Assert.Empty(content.PersonalFolders);
        Assert.NotEmpty(content.PublicFolders);
        Assert.All(content.PublicFolders, folder => Assert.Equal(AccessEnum.Public.ToString(), folder.Access.Name));
    }

    [Fact]
    public async Task GetFolders_ReturnBothPersonalAndPublicFolders_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.GetAsync("api/v1/Folders");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFoldersResponse>();

        Assert.NotNull(content);
        Assert.NotEmpty(content.PersonalFolders);
        Assert.NotEmpty(content.PublicFolders);
        Assert.All(content.PublicFolders, folder => Assert.Equal(AccessEnum.Public.ToString(), folder.Access.Name));
        Assert.Contains(content.PersonalFolders, folder => folder.Access.Name == AccessEnum.Private.ToString());
    }

    [Fact]
    public async Task GetFolders_ReturnCorrectPersonalFolders_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.GetAsync("api/v1/Folders");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFoldersResponse>();

        Assert.NotNull(content);
        Assert.Contains(content.PersonalFolders, folder => folder.Guid == Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49"));
    }

    [Fact]
    public async Task GetFolders_ReturnLimitedPublicFolders_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.GetAsync("api/v1/Folders");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFoldersResponse>();

        Assert.NotNull(content);
        Assert.True(content.PublicFolders.Count <= 50);
        Assert.All(content.PublicFolders, folder => 
        {
            Assert.Equal(AccessEnum.Public.ToString(), folder.Access.Name);
        });
    }
}