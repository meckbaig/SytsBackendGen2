using SytsBackendGen2.Application.Services.Kontragents;
using SytsBackendGen2.Domain.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SytsBackendGen2.Application.IntegrationTests.Controllers.Kontragents;

public class GetListTest
{
    private readonly HttpClient _client;
    public GetListTest()
    {
        var app = new MockesuWebApplicationFactory();

        _client = app.CreateClient();
    }

    [Fact]
    public async Task GetList_ReturnsListOf10_WhenTake10()
    {
        // Arrange

        // Act
        _client.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", TestAuth.GetToken(Permission.ReadMember));

        var response = await _client.GetAsync("api/v1/Kontragents/Get?take=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetKontragentsResponse>();
        Assert.Equal(content.Items.Count, 10);
    }

    [Fact]
    public async Task GetList_ReturnsListOfKontragentsWithPositiveBalanse_WhenFilterBalanseGreaterThanOrEquals0()
    {
        // Arrange

        // Act
        _client.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", TestAuth.GetToken(Permission.ReadMember));

        var response = await _client.GetAsync("api/v1/Kontragents/Get?filters=balance:0..");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetKontragentsResponse>();
        Assert.Equal(0, content.Items.Where(k => k.Balance < 0).Count());
    }

    [Fact]
    public async Task GetList_ReturnsListOfSingle_WhenFilterIdEquals10()
    {
        // Arrange

        // Act
        _client.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", TestAuth.GetToken(Permission.ReadMember));

        var response = await _client.GetAsync("api/v1/Kontragents/Get?filters=id:10");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<GetKontragentsResponse>();
        Assert.Equal(1, content.Items.Count());
        Assert.Equal(10, content.Items[0].Id);
    }

    [Fact]
    public async Task GetList_Returns401_WhenAuthorizationIsMissing()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("api/v1/Kontragents/Get");

        // Assert
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}