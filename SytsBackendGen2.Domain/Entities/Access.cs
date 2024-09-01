using System.ComponentModel.DataAnnotations;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Domain.Entities;

public class Access : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public Access() { }
    public Access(AccessEnum @enum)
    {
        Id = (int)@enum;
        Name = @enum.ToString();
    }

    public static implicit operator Access(AccessEnum @enum) => new Access(@enum);
    public static implicit operator AccessEnum(Access access) => (AccessEnum)access.Id;
}
