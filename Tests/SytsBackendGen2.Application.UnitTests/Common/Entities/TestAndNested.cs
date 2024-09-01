using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.UnitTests.Common.Entities;

public class TestAndNested
{
    [Required]
    [ForeignKey(nameof(TestEntity))]
    public int TestEntityId { get; set; }

    public TestEntity TestEntity { get; set; }

    [Required]
    [ForeignKey(nameof(TestNestedEntity))]
    public int TestNestedEntityId { get; set; }

    public TestNestedEntity TestNestedEntity { get; set; }
}