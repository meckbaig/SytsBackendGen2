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
using SytsBackendGen2.Application.Services.Users;
using SytsBackendGen2.Application.Services.Authorization;
using SytsBackendGen2.Application.DTOs.Users;

namespace SytsBackendGen2.Application.SystemTests.Controllers.Users
{
    public class UpdateYoutubeIdTests : IClassFixture<SytsBackendGen2WithNewDBsWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly SytsBackendGen2WithNewDBsWebApplicationFactory _factory;

        public UpdateYoutubeIdTests(SytsBackendGen2WithNewDBsWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task UpdateYoutubeId_ReturnSuccess_WhenAuthenticated()
        {
            // Arrange
            var command = new UpdateYoutubeIdCommand { youtubeId = "BCOByaobXOl6YYxUNBxSwe_7" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

            // Act
            var response = await _client.PutAsJsonAsync("api/v1/Users/UpdateYoutubeId", command);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify the YouTube ID was updated using the Authorization endpoint
            var authResponse = await AuthorizeUser("valid_access_token");
            Assert.NotNull(authResponse);
            Assert.Equal("BCOByaobXOl6YYxUNBxSwe_7", authResponse.UserData.YoutubeId);
        }

        [Fact]
        public async Task UpdateYoutubeId_ReturnUnauthorized_WhenNotAuthenticated()
        {
            // Arrange
            var command = new UpdateYoutubeIdCommand { youtubeId = "BCOByaobXOl6YYxUNBxSwe_7" };
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PutAsJsonAsync("api/v1/Users/UpdateYoutubeId", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateYoutubeId_ReturnBadRequest_WhenInvalidYoutubeId()
        {
            // Arrange
            var command = new UpdateYoutubeIdCommand { youtubeId = "BCOByaobXOl6Y" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestAuth.MainUserToken);

            // Act
            var response = await _client.PutAsJsonAsync("api/v1/Users/UpdateYoutubeId", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("youtubeId", content);
        }

        private async Task<AuthorizeUserResponse> AuthorizeUser(string accessToken)
        {
            var response = await _client.GetAsync($"api/v1/Authorization?accessToken={accessToken}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthorizeUserResponse>();
        }
    }
}
