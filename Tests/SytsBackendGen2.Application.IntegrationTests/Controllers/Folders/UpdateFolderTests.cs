using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Folders;

public class UpdateFolderTests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

    public UpdateFolderTests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task UpdateFolder_ReturnSuccess_WhenAuthenticated()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = "Updated Test Folder",
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "#ff0000",
                Icon = "new-icon",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{folderGuid}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<UpdateFolderResponse>();
        //var contentString = await response.Content.ReadAsStringAsync();
        //var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.NotNull(content);
        Assert.NotEqual((int)response.StatusCode, 500);
        Assert.Equal(updateRequest.folder.Name, content.Folder.Name);
        Assert.Equal(updateRequest.folder.Color, content.Folder.Color);
        Assert.Equal(updateRequest.folder.Icon, content.Folder.Icon);
        Assert.Equal(updateRequest.folder.Access.Name, content.Folder.Access.Name);
        Assert.Equal(updateRequest.folder.SubChannels.Count, content.Folder.SubChannels.Count);
        Assert.Equal(updateRequest.folder.SubChannels[0].Title, content.Folder.SubChannels[0].Title);
        Assert.Equal(updateRequest.folder.SubChannels[0].ThumbnailUrl, content.Folder.SubChannels[0].ThumbnailUrl);
        Assert.Equal(updateRequest.folder.SubChannels[0].ChannelId, content.Folder.SubChannels[0].ChannelId);
    }

    [Fact]
    public async Task UpdateFolder_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = "Updated Test Folder",
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "#ff0000",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{folderGuid}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFolder_ReturnBadRequest_WhenFolderDoesNotExist()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);
        var nonExistentGuid = Guid.NewGuid();

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = "Updated Test Folder",
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "#ff0000",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{nonExistentGuid}", updateRequest);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("FolderDoesNotExist", content.errors.guid[0].code.ToString());
    }

    [Fact]
    public async Task UpdateFolder_ReturnForbidden_WhenUserIsNotOwner()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.SecondUserToken);

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = "Updated Test Folder",
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "#ff0000",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{folderGuid}", updateRequest);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("ForbiddenAccessValidator", content.errors.guid[0].code.ToString());
    }

    [Fact]
    public async Task UpdateFolder_ReturnBadRequest_WhenNameIsTooLong()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = new string('A', 51), // 51 characters, which is more than the allowed 50
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "#ff0000",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{folderGuid}", updateRequest);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("MaximumLengthValidator", content.errors["folder.name"][0].code.ToString());
    }

    [Fact]
    public async Task UpdateFolder_ReturnBadRequest_WhenColorIsInvalid()
    {
        // Arrange
        Guid folderGuid = await CreateNewFolder();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var updateRequest = new UpdateFolderCommand
        {
            folder = new FolderEditDto
            {
                Name = "Test Folder",
                SubChannels = new List<SubChannelDto>
                {
                    new SubChannelDto
                    {
                        Title = "Test Channel",
                        ThumbnailUrl = "https://example.com/thumbnail.jpg",
                        ChannelId = "UC1234567890"
                    }
                },
                Color = "invalid-color",
                YoutubeFolders = ["videos", "streams"],
                Access = new AccessDto { Id = 2, Name = "Private" }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"api/v1/Folders/{folderGuid}", updateRequest);

        // Assert
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<dynamic>(contentString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("RegularExpressionValidator", content.errors["folder.color"][0].code.ToString());
    }

    private async Task<Guid> CreateNewFolder()
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

        var createRequest = new CreateFolderCommand { name = "Test Folder" };
        var createResponse = await _client.PostAsJsonAsync("api/v1/Folders", createRequest);
        var createdFolder = await createResponse.Content.ReadFromJsonAsync<CreateFolderResponse>();
        return createdFolder.Folder.Guid;
    }
}