using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace SytsBackendGen2.Domain.Entities.Authentification;

[NotCached]
public class Permission : BaseEntity, IEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [DatabaseRelation(Relation.ManyToMany)]
    public HashSet<Role> Roles { get; set; }

    public static implicit operator Permission(Enums.Permission perm)
        => new Permission { Id = (int)perm, Name = perm.ToString() };

    public static implicit operator Permission(int permId)
        => new Permission { Id = permId, Name = ((Enums.Permission)permId).ToString() };
    
    public static implicit operator Permission(string permName)
        => new Permission { Id = (int)Enum.Parse(typeof(Enums.Permission), permName), Name = permName };
}
