using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.DTOs.Users;
using SytsBackendGen2.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace SytsBackendGen2.Application.SystemTests.Controllers
{
    public class SytsBackendGen2WithNewDBsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private PostgreSqlContainer _container;
        private DbConnection _connection;

        public async Task InitializeAsync()
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("SytsBackendGen2")
                .WithUsername("postgres")
                .WithPassword("testtest")
                .Build();

            await _container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>((container, options) =>
                {
                    options.UseNpgsql(_container.GetConnectionString());
                });

                services.AddSingleton<DbConnection>(provider =>
                {
                    _connection = new Npgsql.NpgsqlConnection(_container.GetConnectionString());
                    _connection.Open();
                    return _connection;
                });

                // Remove the real IGoogleAuthProvider and add the mock
                var descriptorGoogle = services.SingleOrDefault(d => d.ServiceType == typeof(IGoogleAuthProvider));
                if (descriptorGoogle != null)
                    services.Remove(descriptorGoogle);
                services.AddSingleton<IGoogleAuthProvider, MockGoogleAuthProvider>();

                // Seed the database after it's created
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    SeedDatabase(dbContext);
                }
            });

            base.ConfigureWebHost(builder);
        }

        private void SeedDatabase(AppDbContext context)
        {
            // Read the SQL script
            string script = File.ReadAllText("../../../dump-test-database.sql");

            context.Database.ExecuteSqlRaw(script);

        }

        public void ResetDatabase()
        {
            using (var scope = Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                SeedDatabase(context);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Dispose();
                _container?.DisposeAsync().AsTask().Wait();
            }
            base.Dispose(disposing);
        }
    }
}

public class MockGoogleAuthProvider : IGoogleAuthProvider
{
    private bool _multiplePages = false;
    private int _pageCount = 0;
    private string _invalidAccessToken = null;
    private string _newUserEmail = null;

    public Task<UserPreviewDto?> AuthorizeAsync(string accessToken)
    {
        if (accessToken == _invalidAccessToken)
        {
            throw new HttpRequestException("Invalid access token");
        }

        var userPreview = new UserPreviewDto(
            "Test User", 
            _newUserEmail ?? "first@gmail.com", 
            "https://example.com/profile.jpg"
        );
        return Task.FromResult<UserPreviewDto?>(userPreview);
    }

    public Task<string> GetYoutubeIdByName(string username)
    {
        return Task.FromResult("UCOByaobXOl6YYxUNBxSwe_7");
    }

    public Task<(List<SubChannelDto>, string?)> GetSubChannels(string channelId, string? nextPageToken = null)
    {
        if (!_multiplePages)
        {
            var subChannel = new SubChannelDto
            {
                Title = "Test Channel",
                ThumbnailUrl = "https://example.com/thumbnail.jpg",
                ChannelId = "UC1234567890"
            };

            return Task.FromResult((new List<SubChannelDto> { subChannel }, (string?)null));
        }
        else
        {
            _pageCount++;
            var subChannels = new List<SubChannelDto>
            {
                new SubChannelDto
                {
                    Title = $"Test Channel {_pageCount}",
                    ThumbnailUrl = $"https://example.com/thumbnail{_pageCount}.jpg",
                    ChannelId = $"UC123456789{_pageCount}"
                }
            };

            string? nextToken = _pageCount < 3 ? "next_page_token" : null;
            return Task.FromResult((subChannels, nextToken));
        }
    }

    public void SetMultiplePages(bool value)
    {
        _multiplePages = value;
        _pageCount = 0;
    }

    public void SetInvalidAccessToken(string invalidToken)
    {
        _invalidAccessToken = invalidToken;
    }

    public void SetNewUser(string email)
    {
        _newUserEmail = email;
    }
}