using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.IntegrationTests.Controllers.Folders;

public class GetFolderTests
{
    private readonly HttpClient _client;
    public GetFolderTests()
    {
        var app = new SytsBackendGen2WebApplicationFactory();

        _client = app.CreateClient();
    }

    [Fact]
    public async Task GetFolder_ReturnFullData_WhenNotAuthenticated()
    {
        // Arrange
        Guid guid = Guid.Parse("2f6a133e-dab6-4ec3-85f4-0f2654d7c628");
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync($"api/v1/Folders/{guid}");

        // Assert
        response.EnsureSuccessStatusCode();

        var sadas = await response.Content.ReadAsStringAsync();
        var content = await response.Content.ReadFromJsonAsync<GetFolderResponse>();

        Assert.NotNull(content);
        Assert.Equal(content.Folder.Guid, guid);
        Assert.Equal(content.Folder.Name, "Аква и Това");
        Assert.Equal(content.Folder.Access.Name, AccessEnum.LinkAccess.ToString());
        Assert.True(content.Videos.Count > 0);
    }
}
