using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Domain.Entities;

public class Folder : BaseEntity
{
    [Required]
    public Guid Guid { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [ForeignKey(nameof(Access))]
    public int AccessId { get; set; } = 2;

    public DateTime? LastChannelsUpdate { get; set; }

    [Required]
    [Column(TypeName = "json")]
    public string SubChannelsJson { get; set; }

    [Required]
    public int ChannelsCount { get; set; } = 0;

    [Required]
    public string? Color { get; set; } = "#ffffff";

    public string? Icon { get; set; }

    [StringLength(11)]
    public string? LastVideoId { get; set; }

    [Required]
    [Column(TypeName = "json")]
    public string YoutubeFolders { get; set; } = "[\"videos\", \"streams\"]";

    public User User { get; set; }
    public Access Access { get; set; }

    public Folder() { }
    public Folder(int userId, string name, string? subChannelsJson = "")
    {
        Guid = Guid.NewGuid();
        Name = name;
        UserId = userId;
        if (subChannelsJson != "")
            SetSubChannelsJson(subChannelsJson);
    }

    public void SetSubChannelsJson(string subChannelsJson)
    {
        SubChannelsJson = subChannelsJson;
        LastChannelsUpdate = DateTime.UtcNow; //DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    }
}