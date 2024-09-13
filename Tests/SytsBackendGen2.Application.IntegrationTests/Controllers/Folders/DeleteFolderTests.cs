using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Folders;

public class DeleteFolderTests : IClassFixture<SytsBackendGen2WebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WebApplicationFactory _factory;

    public DeleteFolderTests()
    {
        _factory = new SytsBackendGen2WebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task DeleteFolder_ReturnSuccess_WhenAuthenticated()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        // Act
        var response = await _client.DeleteAsync($"api/v1/Folders/{folderGuid}");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteFolder_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.DeleteAsync($"api/v1/Folders/{folderGuid}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFolder_ReturnNotFound_WhenFolderDoesNotExist()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);
        var nonExistentGuid = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"api/v1/Folders/{nonExistentGuid}");

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal((int)response.StatusCode, 400);
        Assert.Equal(content.errors.guid[0].code.ToString(), "FolderDoesNotExist");
    }

    [Fact]
    public async Task DeleteFolder_ReturnForbidden_WhenUserIsNotOwner()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.SecondUserToken);

        // Act
        var response = await _client.DeleteAsync($"api/v1/Folders/{folderGuid}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateNewFolder()
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var createRequest = new CreateFolderCommand { name = "Test Folder" };
        var createResponse = await _client.PostAsJsonAsync("api/v1/Folders", createRequest);
        var createdFolder = await createResponse.Content.ReadFromJsonAsync<CreateFolderResponse>();
        var folderGuid = createdFolder.Folder.Guid;
        return folderGuid;
    }

    public void Dispose()
    {
        _factory.ResetDatabase();
    }
}
