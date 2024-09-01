using SytsBackendGen2.Application.UnitTests.Common;
using Testcontainers.PostgreSql;

namespace SytsBackendGen2.Application.UnitTests.JsonPatch;

public class TestWithContainer
{
    private static PostgreSqlContainer _container;
    internal static PostgreSqlContainer Container
    {
        get
        {
            _container ??= new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("SytsBackendGen2")
                .WithUsername("postgres")
                .WithPassword("testtest")
                //.WithPortBinding(5555, 5432)
                //.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
            return _container;
        }
    }

    private static TestDbContext _context;
    internal static TestDbContext Context
    {
        get
        {
            _context ??= JsonPatchTestHelper.CreateAndSeedContext(Container).Result;
            return _context;
        }
    }
}
