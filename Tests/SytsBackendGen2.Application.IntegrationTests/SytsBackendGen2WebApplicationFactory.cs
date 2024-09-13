using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SytsBackendGen2.Infrastructure.Data;
using System.Diagnostics;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace SytsBackendGen2.Application.SystemTests;

public class SytsBackendGen2WebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection _connection;
    private DbTransaction _transaction;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseNpgsql(connection);
            });

            services.AddSingleton<DbConnection>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");;
                _connection = new Npgsql.NpgsqlConnection(connectionString);
                _connection.Open();
                _transaction = _connection.BeginTransaction();
                return _connection;
            });
        });

        base.ConfigureWebHost(builder);
    }

    public void ResetDatabase()
    {
        _transaction.Rollback();
        _transaction = _connection.BeginTransaction();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
