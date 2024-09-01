using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Volumes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace SytsBackendGen2.Application.Tests.Services.Kontragents;

//public class KontragentsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
//{
//    protected readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
//        .WithImage("postgres:latest")
//        .WithDatabase("SytsBackendGen2")
//        .WithUsername("postgres")
//        .WithPassword("testtest")
//        .WithPortBinding(5555, 5432)
//        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
//        .Build();
    
//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.ConfigureTestServices(services =>
//        {
//            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));
    
//            if (descriptor is not null)
//                services.Remove(descriptor);
    
//            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_container.GetConnectionString()));
//        });
//    }

//    // protected void PutDataIntoMockDatabase(string originalConntectionString, IAppDbContext mock)
//    // {
//    //     var original = new AppDbContext(
//    //         new DbContextOptionsBuilder<AppDbContext>()
//    //             .UseNpgsql(connectionString: originalConntectionString)
//    //             .Options);
//    //     var kontragents = original.Kontragents
//    //         .Include(k => k.Address).ThenInclude(a => a.Street).ThenInclude(s => s.City)
//    //         .Include(k => k.Address).ThenInclude(a => a.Region)
//    //         .Include(k => k.KontragentAgreement).ThenInclude(ka => ka.Organization)
//    //         .Include(k => k.KontragentAgreement).ThenInclude(ka => ka.PaymentContract)
//    //         .ToList();
//    //     mock.Kontragents.AddRange(kontragents);
//    //     mock.SaveChanges();
//    // }

//    public async Task InitializeAsync()
//    {
//        await _container.StartAsync();
//    }

//    public new async Task DisposeAsync()
//    {
//        await _container.StopAsync();
//    }
//}
