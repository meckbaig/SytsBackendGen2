using SytsBackendGen2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SytsBackendGen2.Application.UnitTests.Common.Entities;

public class TestNestedEntity : BaseEntity, IEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }

    public double Number { get; set; }

    [DatabaseRelation(Domain.Enums.Relation.ManyToMany)]
    public HashSet<TestEntity> TestEntities { get; set; }
}
