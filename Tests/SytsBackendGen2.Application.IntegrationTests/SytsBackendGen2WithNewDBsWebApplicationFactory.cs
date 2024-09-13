using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
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
