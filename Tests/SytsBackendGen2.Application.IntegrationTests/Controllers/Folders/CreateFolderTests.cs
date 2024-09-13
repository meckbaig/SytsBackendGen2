using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Folders;

public class CreateFolderTests : IClassFixture<SytsBackendGen2WebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WebApplicationFactory _factory;

    public CreateFolderTests()
    {
        _factory = new SytsBackendGen2WebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateFolder_ReturnCreatedFolder_WhenAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var request = new CreateFolderCommand
        {
            name = "Test Folder"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/Folders", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateFolderResponse>();

        Assert.NotNull(content);
        Assert.Equal(request.name, content.Folder.Name);
        Assert.Equal(AccessEnum.Private.ToString(), content.Folder.Access.Name);
    }

    [Fact]
    public async Task CreateFolder_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        var request = new CreateFolderCommand
        {
            name = "Test Folder"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/Folders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFolder_ReturnBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var request = new CreateFolderCommand
        {
            name = new string('A', 51) // 51 characters, which is more than the allowed 50
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/Folders", request);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(content.errors.name[0].code.ToString(), "LengthValidator");
    }

    [Fact]
    public async Task CreateFolder_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var request = new CreateFolderCommand
        {
            name = string.Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/Folders", request);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(content.errors.name[0].code.ToString(), "LengthValidator");
    }

    public void Dispose()
    {
        _factory.ResetDatabase();
    }
}