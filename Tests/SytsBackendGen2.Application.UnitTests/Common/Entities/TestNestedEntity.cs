using SytsBackendGen2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.UnitTests.Common.Entities;

public class TestNestedEntity : BaseEntity
{
    public string Name { get; set; }

    public double Number { get; set; }

    [DatabaseRelation(Domain.Enums.Relation.ManyToMany)]
    public HashSet<TestEntity> TestEntities { get; set; }
}
