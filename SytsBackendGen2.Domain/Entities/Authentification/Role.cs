using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Domain.Entities.Authentification;

public class Role : BaseEntity, IEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [DatabaseRelation(Relation.ManyToMany)]
    public HashSet<Permission> Permissions { get; set; } = [];

    public List<User> Users { get; set; }
}
