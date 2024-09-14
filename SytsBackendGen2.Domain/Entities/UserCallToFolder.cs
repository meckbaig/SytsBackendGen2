using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Domain.Entities;

public class UserCallToFolder : BaseEntity
{
    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [ForeignKey(nameof(Folder))]
    public int FolderId { get; set; }

    [StringLength(11)]
    public string? LastVideoId { get; set; }

    public DateTimeOffset LastUserCall { get; set; }

    public User User { get; set; }
    public Folder Folder { get; set; }
}
