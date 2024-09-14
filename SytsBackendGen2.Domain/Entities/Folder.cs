using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Domain.Entities;

public class Folder : BaseEntity, IEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

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

    [Required]
    [Column(TypeName = "json")]
    public string SubChannelsJson { get; set; } = "[]";

    [Required]
    public int ChannelsCount { get; set; } = 0;

    [Required]
    public string? Color { get; set; } = "#ffffff";

    public string? Icon { get; set; }

    [Required]
    [Column(TypeName = "json")]
    public string YoutubeFolders { get; set; } = "[\"videos\", \"streams\"]";

    public User User { get; set; }
    public List<UserCallToFolder> UsersCallsToFolder { get; set; }
    public Access Access { get; set; }

    private int _currentUserId = 0;

    public Folder() { }
    public Folder(int userId, string name, string? subChannelsJson = null)
    {
        Guid = Guid.NewGuid();
        Name = name;
        UserId = userId;
        if (subChannelsJson != null) 
            SubChannelsJson = subChannelsJson;
    }

    /// <summary>
    /// It is necessary to call this method before projecting Folder model to a DTO.
    /// </summary>
    /// <param name="userId">User id.</param>
    public void SetCurrentUserId(int userId) => _currentUserId = userId;

    public void SetLastVideoId(int userId, string? lastVideoId)
    {
        UserCallToFolder? userCallToFolder = GetOrCreateLastVideoInFolder(userId);
        userCallToFolder.LastVideoId = lastVideoId;
    }

    public DateTimeOffset SetLastVideosCall(int userId)
    {
        UserCallToFolder? userCallToFolder = GetOrCreateLastVideoInFolder(userId);
        userCallToFolder.LastUserCall = DateTimeOffset.UtcNow;
        return userCallToFolder.LastUserCall;
    }

    public string? GetLastVideoId(int userId)
    {
        UserCallToFolder? userCallToFolder = GetOrCreateLastVideoInFolder(userId);
        return userCallToFolder.LastVideoId;
    }

    public DateTimeOffset? GetLastVideosCall() => GetLastVideosCall(_currentUserId);

    public DateTimeOffset? GetLastVideosCall(int userId)
    {
        UserCallToFolder? userCallToFolder = GetOrCreateLastVideoInFolder(userId);
        return userCallToFolder.LastUserCall;
    }

    private UserCallToFolder GetOrCreateLastVideoInFolder(int userId)
    {
        if (userId == 0)
            return new UserCallToFolder();
        var userCallToFolder = UsersCallsToFolder.FirstOrDefault(lv => lv.UserId == userId);
        if (userCallToFolder == null)
        {
            userCallToFolder = new UserCallToFolder() { UserId = userId, FolderId = Id };
            UsersCallsToFolder.Add(userCallToFolder);
        }
        return userCallToFolder;
    }
}