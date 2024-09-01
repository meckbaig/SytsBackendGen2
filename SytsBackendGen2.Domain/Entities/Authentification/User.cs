using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SytsBackendGen2.Domain.Common;

namespace SytsBackendGen2.Domain.Entities.Authentification;

public class User : BaseEntity, INonDelitableEntity
{
    [Required]
    public Guid Guid { get; set; }

    [Required]
    [StringLength(320)]
    public string Email { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    [Required]
    public bool Deleted { get; set; }

    [Required]
    [StringLength(24)]
    public string YoutubeId { get; set; }

    [Required]
    [Column(TypeName = "json")]
    public string? SubChannelsJson { get; set; }

    public DateTime? LastChannelsUpdate { get; set; }

    public Role Role { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; }
    public List<Folder> Folders { get; set; }

    public User() { }
    public User(string youtubeId, string email)
    {
        Guid = Guid.NewGuid();
        YoutubeId = youtubeId;
        Email = email;
        RoleId = 1;
    }

    public void SetSubChannelsJson(string subChannelsJson)
    {
        SubChannelsJson = subChannelsJson;
        LastChannelsUpdate = DateTime.UtcNow;
    }
}
