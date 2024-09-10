using Newtonsoft.Json.Linq;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IVideoFetcher
{
    public Task<bool> Fetch(JArray subChannels, JArray youtubeFolders, int channelsCount);
    public List<dynamic> ToList(string lastVideoId = "");
}
