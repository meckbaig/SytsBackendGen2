using SytsBackendGen2.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SytsBackendGen2.Domain.Entities.Authentification;

[NotCached]
public class PermissionInRole
{
    [Required]
    [ForeignKey(nameof(Permission))]
    public int PermissionId { get; set; }

    public Permission Permission { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    public Role Role { get; set; }
}
