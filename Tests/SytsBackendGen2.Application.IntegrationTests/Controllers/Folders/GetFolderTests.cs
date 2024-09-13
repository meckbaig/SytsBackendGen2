using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Folders;

public class GetFolderTests
{
    private readonly HttpClient _client;
    public GetFolderTests()
    {
        var app = new SytsBackendGen2WebApplicationFactory();

        _client = app.CreateClient();
    }

    [Fact]
    public async Task GetFolder_ReturnFullData_WhenNotAuthenticatedCallToLinkAccessFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("2f6a133e-dab6-4ec3-85f4-0f2654d7c628");
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFolderResponse>();

        Assert.NotNull(content);
        Assert.Equal(content.Folder.Guid, guid);
        Assert.Equal(content.Folder.Name, "Аква и Това");
        Assert.Equal(content.Folder.Access.Name, AccessEnum.LinkAccess.ToString());
        Assert.True(content.Videos.Count > 0);
    }

    [Fact]
    public async Task GetFolder_Return403_WhenNotAuthenticatedCallToPrivateFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert

        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.NotNull(content);
        Assert.Equal((int)response.StatusCode, 403);

        Assert.Equal(content.errors.jwtToken[0].code.ToString(), "ForbiddenAccessValidator");
    }

    [Fact]
    public async Task GetFolder_Return400_WhenNotAuthenticatedCallToNonexistentFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a9-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert

        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.NotNull(content);
        Assert.Equal((int)response.StatusCode, 400);

        Assert.Equal(content.errors.guid[0].code.ToString(), "FolderDoesNotExist");
    }

    [Fact]
    public async Task GetFolder_ReturnFullData_WhenAuthenticatedCallToOwnPrivateFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFolderResponse>();

        Assert.NotNull(content);
        Assert.Equal(content.Folder.Guid, guid);
        Assert.Equal(content.Folder.Access.Name, AccessEnum.Private.ToString());
        Assert.True(content.Videos.Count > 0);
    }

    [Fact]
    public async Task GetFolder_Return403_WhenAuthenticatedCallToOtherUsersPrivateFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.SecondUserToken);

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.NotNull(content);
        Assert.Equal((int)response.StatusCode, 403);
        Assert.Equal(content.errors.jwtToken[0].code.ToString(), "ForbiddenAccessValidator");
    }

    [Fact]
    public async Task GetFolder_ReturnFullData_WhenAuthenticatedCallToEditOwnFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}?toEdit=true");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetFolderResponse>();

        Assert.NotNull(content);
        Assert.Equal(content.Folder.Guid, guid);
        Assert.Null(content.Videos); // Videos should be null when toEdit is true
    }

    [Fact]
    public async Task GetFolder_Return403_WhenAuthenticatedCallToEditOtherUsersFolder()
    {
        // Arrange
        Guid guid = Guid.Parse("236c69bf-6fc6-4768-a9a8-1bef149b7c49");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.SecondUserToken);

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}?toEdit=true");

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.NotNull(content);
        Assert.Equal((int)response.StatusCode, 403);
        Assert.Equal(content.errors.jwtToken[0].code.ToString(), "ForbiddenAccessValidator");
    }
}
