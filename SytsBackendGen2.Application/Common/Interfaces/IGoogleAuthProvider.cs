using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.DTOs.Users;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IGoogleAuthProvider
{
    Task<UserPreviewDto?> AuthorizeAsync(string accessToken);
    Task<string> GetYoutubeIdByName(string username);
    Task<(List<SubChannelDto>, string?)> GetSubChannels(string channelId, string? nextPageToken = null);
}
