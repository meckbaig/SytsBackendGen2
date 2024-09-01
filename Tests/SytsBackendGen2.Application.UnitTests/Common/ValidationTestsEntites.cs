using SytsBackendGen2.Application.UnitTests.Common.Entities;

namespace SytsBackendGen2.Application.UnitTests.Common;

public static class ValidationTestsEntites
{
    public static IQueryable<TestEntity> SeedTestEntities(int count = 10)
    {
        List<TestEntity> entities = new List<TestEntity>();

        int itemsCount = count != 0 ? count : 10;
        for (int i = 0; i < itemsCount; i++)
        {
            var entity = new TestEntity
            {
                Id = i,
                Name = $"Name{i}",
                Description = $"Description{i}",
                Date = DateOnly.FromDateTime(new DateTime(2024, 1, 1).AddDays(-i)),
                SomeCount = 100 - i * 10,
                TestNestedEntities = new HashSet<TestNestedEntity>
                {
                    new() {
                        Id = i,
                        Name = $"NestedName{i}",
                        Number = i * 1.5
                    },
                    new() {
                        Id = i+1,
                        Name = $"NestedName{i+1}",
                        Number = (i+1) * 1.5
                    },
                    new() {
                        Id = i+2,
                        Name = $"NestedName{i+2}",
                        Number = (i+2) * 1.5
                    }
                },
                InnerEntity = new()
                {
                    Id = 100 + i,
                    Name = $"NestedName{100 + i}",
                    Number = (100 + i) * 1.5
                },
                InnerEntityId = 100 + i
            };

            entities.Add(entity);
        }

        return entities.AsQueryable();
    }
}


