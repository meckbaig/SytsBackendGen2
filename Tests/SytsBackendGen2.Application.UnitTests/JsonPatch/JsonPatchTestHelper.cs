using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.UnitTests.Common;
using SytsBackendGen2.Application.UnitTests.Common.Entities;
using Testcontainers.PostgreSql;

namespace SytsBackendGen2.Application.UnitTests.JsonPatch;

internal static class JsonPatchTestHelper
{

    internal static async Task<TestDbContext> CreateAndSeedContext(PostgreSqlContainer container)
    {
        await container.StartAsync();
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseNpgsql(container.GetConnectionString());
        var context = new TestDbContext(optionsBuilder.Options);
        TestMigration(context);
        return context;
    }

    internal static void TestMigration(TestDbContext context, int count = 10)
    {
        try
        {
            var TestNestedEntities = GetTestNestedEntities(count);
            var TestEntities = GetTestEntities(count);
            var TestAndNesteds = GetTestAndNestedEntities(count);

            InitScript(context);
            context.TestNestedEntities.AddRange(TestNestedEntities);
            context.TestEntities.AddRange(TestEntities);
            context.TestAndNesteds.AddRange(TestAndNesteds);
            context.SaveChanges();

        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private static void InitScript(TestDbContext context)
    {
        string script = File.ReadAllText(@"..\..\..\dump-unit-test.sql");

        context.Database.ExecuteSqlRaw(script);
    }

    public static List<TestEntity> GetTestEntities(int count = 10)
    {
        List<TestEntity> entities = new List<TestEntity>();

        int itemsCount = count != 0 ? count : 10;
        for (int i = 1; i <= itemsCount; i++)
        {
            var entity = new TestEntity
            {
                Id = i,
                Name = $"Name{i}",
                Description = $"Description{i}",
                Date = DateOnly.FromDateTime(new DateTime(2024, 1, 1).AddDays(-i)),
                SomeCount = 100 - i * 10,
                TestNestedEntities = new HashSet<TestNestedEntity>(),
                InnerEntityId = 100 + i
            };

            entities.Add(entity);
        }

        return entities;
    }

    public static HashSet<TestNestedEntity> GetTestNestedEntities(int count = 10)
    {
        HashSet<TestNestedEntity> entities = new HashSet<TestNestedEntity>();

        int itemsCount = count != 0 ? count : 10;
        for (int i = 1; i <= itemsCount + 2; i++)
        {
            var entity = new TestNestedEntity
            {
                Id = i,
                Name = $"NestedName{i}",
                Number = i * 1.5
            };
            entities.Add(entity);

            if (!entities.Any(x => x.Id == 100 + i))
            {
                var entity100 = new TestNestedEntity
                {
                    Id = 100 + i,
                    Name = $"NestedName{100 + i}",
                    Number = (100 + i) * 1.5
                };
                entities.Add(entity100);
            }
        }
        return entities;
    }

    public static List<TestAndNested> GetTestAndNestedEntities(int count = 10)
    {
        List<TestAndNested> entities = new List<TestAndNested>();

        int itemsCount = count != 0 ? count : 10;
        for (int i = 1; i <= itemsCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var entity = new TestAndNested
                {
                    TestEntityId = i,
                    TestNestedEntityId = i + j,
                };

                entities.Add(entity);
            }
        }

        return entities;
    }
}
