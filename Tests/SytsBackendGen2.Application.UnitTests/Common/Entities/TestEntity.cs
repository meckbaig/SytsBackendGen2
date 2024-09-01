using SytsBackendGen2.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SytsBackendGen2.Application.UnitTests.Common.Entities;

public class TestEntity : BaseEntity
{
    public string Name { get; set; }

    public string Description { get; set; }

    public DateOnly Date { get; set; }

    public int SomeCount { get; set; }

    [DatabaseRelation(Domain.Enums.Relation.ManyToMany)]
    public HashSet<TestNestedEntity> TestNestedEntities { get; set; }

    [ForeignKey(nameof(InnerEntityId))]
    public TestNestedEntity InnerEntity { get; set; }

    public int InnerEntityId { get; set; }
}
