using Newtonsoft.Json.Linq;
using SytsBackendGen2.Application.DTOs.Folders;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IVideoFetcher
{
    public Task<bool> Fetch(List<SubChannelDto> subChannels, string[] youtubeFolders, int channelsCount);
    public List<dynamic> ToList(string lastVideoId = "");
}
