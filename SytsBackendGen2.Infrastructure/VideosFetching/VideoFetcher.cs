using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SytsBackendGen2.Infrastructure.VideosFetching;

/// <summary>
/// Legacy youtube video fetcher.
/// </summary>
public class VideoFetcher : IVideoFetcher
{
    private readonly HttpClient _httpClient;
    private readonly string _youtubeKey;
    private string[] _youtubeFolders;
    private int _foldersCount;

    private int _videosPerChannelFolder;
    const int _ytRequestVideosCount = 50;

    List<dynamic> _notReadyVideosList = new List<dynamic>();
    List<dynamic> _readyVideosList = new List<dynamic>();
    bool _allTasksSet = false;
    int _totalVideosCount = 0;
    int _appendCallsCount = 0;

    object _notReadyVideosListLocker = new();
    object _readyVideosListAddRangeLocker = new();

    public VideoFetcher(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _youtubeKey = configuration["YoutubeKey"];
    }

    private void Initialise(JArray youtubeFolders, int channelsCount)
    {
        try
        {
            _youtubeFolders = youtubeFolders.Select(x => (string)x).ToArray();
            
            if ((_youtubeFolders?.Length ?? 0) < 1)
                _youtubeFolders = ["videos", "streams"];
            if (channelsCount > 0)
            {
                _videosPerChannelFolder = 800 / channelsCount / _youtubeFolders.Length;
                _foldersCount = channelsCount * _youtubeFolders.Length;
            }
            else
                _foldersCount = _videosPerChannelFolder = 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Операция формирования списка видео
    /// </summary>
    /// <returns>Статус выполнения операции</returns>
    public async Task<bool> Fetch(JArray subChannels, JArray youtubeFolders, int channelsCount)
    {
        Initialise(youtubeFolders, channelsCount);
        try
        {
            if (_foldersCount == 0)
                return false;
            List<Task> channelsTasks = new();
            foreach (dynamic channelJson in subChannels)
            {
                channelsTasks.Add(GetChannelVideos(channelJson));
            }
            await Task.WhenAll(channelsTasks);
            List<Task> appendDataTasks = new();
            int notReadyVideosCount = _notReadyVideosList.Count;
            while (notReadyVideosCount > 0)
            {
                int take = notReadyVideosCount > _ytRequestVideosCount ? _ytRequestVideosCount : notReadyVideosCount;
                notReadyVideosCount -= take;
                Task task = Task.Run(async () => await AppendData(take));
                appendDataTasks.Add(task);
            }
            await Task.WhenAll(appendDataTasks);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    /// Запуск операции на получение видео с канала
    /// </summary>
    /// <param name="channelJson">Объект канала</param>
    /// <remarks>Объект должен содержать идентификатор, название и иконку канала</remarks>
    /// <returns></returns>
    private async Task GetChannelVideos(dynamic channelJson)
    {
        try
        {
            List<Task> channelTasks = new();
            string channelId = channelJson.channelId;
            string thumbnail = channelJson.thumbnailUrl;
            foreach (string ytFolder in _youtubeFolders)
            {
                Task task = Task.Run(async () => await GetFolderVideos(channelId, ytFolder, thumbnail, _videosPerChannelFolder));
                channelTasks.Add(task);
            }
            _allTasksSet = true;
            await Task.WhenAll(channelTasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    /// <summary>
    /// Запуск операции на получение видео с папки канала
    /// </summary>
    /// <param name="channelId">Идентификатор канала</param>
    /// <param name="folder">Папка канала</param>
    /// <param name="channelThumbnail">Иконка канала</param>
    /// <param name="videosPerFolderCount">Количество видео с папки канала</param>
    /// <returns></returns>
    private async Task GetFolderVideos(string channelId, string folder, string channelThumbnail, int videosPerFolderCount = 30)
    {
        try
        {
            object videoAdditionLocker = new();
            List<dynamic> videos = new List<dynamic>();
            List<Task> videosTasks = new List<Task>();
            string html = await GetHtml(channelId, folder);
            if (html == "")
                return;
            dynamic contents = GetContentFromYtHtml(html, folder);
            if (contents == null || (contents as string) == "")
                return;
            int contentsLength = (contents as JArray).Count();
            videosPerFolderCount = videosPerFolderCount > contentsLength ? contentsLength : videosPerFolderCount;
            _totalVideosCount += videosPerFolderCount;
            for (int i = 0; i < videosPerFolderCount; i++)
            {
                dynamic videoContent = contents[i];
                Task task = Task.Run(() => GetVideo(channelId, channelThumbnail, videoAdditionLocker, videos, videoContent));
                videosTasks.Add(task);
            }
            await Task.WhenAll(videosTasks);
            await AddToAppendData(videos);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Добавление данных к списку не готовых к выдаче видео
    /// </summary>
    /// <param name="videos">Список видео для добавления</param>
    /// <returns></returns>
    private async Task AddToAppendData(List<dynamic> videos)
    {
        lock (_notReadyVideosListLocker)
        {
            _notReadyVideosList.AddRange(videos);
        }
        await AppendData();
    }

    /// <summary>
    /// Метод на непринудительное дополнение информации о видео из списка не готовых к выдаче
    /// </summary>
    /// <param name="takeVideos">Количество видео для дополнения</param>
    /// <returns></returns>
    private async Task AppendData(int takeVideos = 0)
    {
        List<dynamic> videosToAppend = new List<dynamic>();
        lock (_notReadyVideosListLocker)
        {
            if (_notReadyVideosList.Count < takeVideos)
                takeVideos = _notReadyVideosList.Count;
            else if (takeVideos == 0 && _notReadyVideosList.Count >= _ytRequestVideosCount)
                takeVideos = _ytRequestVideosCount;
            videosToAppend = _notReadyVideosList.Take(takeVideos).ToList();
            _notReadyVideosList.RemoveRange(0, takeVideos);
        }
        if (videosToAppend.Count > 0)
        {
            _appendCallsCount++;
            await AppendInfoToVideos(videosToAppend);
        }
    }

    /// <summary>
    /// Получение контента из нужного раздела с YouTube
    /// </summary>
    /// <param name="html">Контент от YouTube</param>
    /// <param name="folder">Название папки канала</param>
    /// <returns>Контент из папки канала</returns>
    private dynamic GetContentFromYtHtml(string html, string folder)
    {
        dynamic dynamicObject = JsonConvert.DeserializeObject<dynamic>(html)!;
        dynamic tabs = dynamicObject.contents.twoColumnBrowseResultsRenderer.tabs;

        dynamic contents = "";
        try
        {
            foreach (dynamic tab in tabs)
            {
                if (tab?.tabRenderer != null)
                {
                    string url = tab?.tabRenderer?.endpoint?.commandMetadata?.webCommandMetadata?.url;
                    string title = url.Split('/').Last();
                    if (title.ToLower() == folder.ToLower())
                    {
                        contents = tab?.tabRenderer?.content?.richGridRenderer?.contents;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return contents;
    }

    /// <summary>
    /// Запуск операции на получение видео
    /// </summary>
    /// <param name="channelId">Идентификатор канала</param>
    /// <param name="channelThumbnail">Иконка канала</param>
    /// <param name="videoAdditionLocker">Блокировщик доступа к списку</param>
    /// <param name="videos">Список, в который добавится видео</param>
    /// <param name="videoContent">JSON с контентом от YouTube</param>
    /// <returns></returns>
    private async Task GetVideo(string channelId, string channelThumbnail, object videoAdditionLocker, List<dynamic> videos, dynamic videoContent)
    {
        try
        {
            if ((videoContent?.richItemRenderer as JObject)?.ToString() == null)
                return;
            dynamic content = videoContent.richItemRenderer.content.videoRenderer;
            dynamic video = new System.Dynamic.ExpandoObject();
            video.id = content.videoId;
            video.title = content.title.runs[0].text;
            video.channelId = channelId;
            video.channelThumbnail = channelThumbnail;
            string simpleLength = content.thumbnailOverlays[0].thumbnailOverlayTimeStatusRenderer.text.simpleText;
            video.simpleLength = simpleLength?[0] >= 48 && simpleLength?[0] <= 57 ? simpleLength : "";
            video.viewCount = content.viewCountText?.simpleText;
            string? viewCountText = content.viewCountText?.simpleText;
            if (viewCountText != null)
            {
                StringBuilder viewCount = new StringBuilder();
                foreach (char c in viewCountText)
                {
                    if (c >= 48 && c <= 57)
                    {
                        viewCount.Append(c);
                    }
                    else if (c.ToString() == " " || (c.ToString() == ",")) { viewCount.Append(" "); }
                    else break;
                }
                video.viewCount = viewCount.ToString();
            }
            else
                video.viewCount = "";
            lock (videoAdditionLocker)
            {
                videos.Add(video);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(videoContent);
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Получение данных из HTML запроса к YouTube
    /// </summary>
    /// <param name="channelId">Идентификатор канала</param>
    /// <param name="folder">Папка канала</param>
    /// <returns>Список разделов канала</returns>
    private async Task<string> GetHtml(string channelId, string folder)
    {
        string html = "";
        Task downloadString = Task.Run(async ()
            => html = await _httpClient.GetStringAsync($"https://www.youtube.com/channel/{channelId}/{folder}"));
        string startSubstring = "var ytInitialData = ";
        string endSubstring = ";</script>";
        downloadString.Wait();
        return await GetSubstring(html, startSubstring, endSubstring);
    }

    /// <summary>
    /// Получение подстроки из родительской строки
    /// </summary>
    /// <param name="original">Родительская строка</param>
    /// <param name="start">Начало подстроки</param>
    /// <param name="end">Конец подстроки</param>
    /// <returns>Подстрока</returns>
    private async Task<string> GetSubstring(string original, string start, string end)
    {
        int startId = original.IndexOf(start) + start.Length;
        int endId = original.IndexOf(end, startId);
        if (startId > 0 && endId > 0)
            return original[startId..endId];
        return "";
    }

    /// <summary>
    /// Получение готовых к выдаче видео
    /// </summary>
    /// <returns>Список видео, отсортированных по дате</returns>
    public List<dynamic> ToList(out string firstVideoId, string lastVideoId = "")
    {
        QuickSort();
        foreach (dynamic video in _readyVideosList)
        {
            if (video.id == lastVideoId || lastVideoId == "")
                break;
            video.isNew = true;
        }
        if (_readyVideosList.Count > 0)
            firstVideoId = _readyVideosList?[0]?.id;
        else
            firstVideoId = "";
        return _readyVideosList;
    }

    /// <summary>
    /// Сортировка видео по дате
    /// </summary>
    /// <param name="l">Крайний левый индекс списка</param>
    /// <param name="r">Крайний правый индекс списка</param>
    /// <returns>Отсортированные видео</returns>
    private List<dynamic> QuickSort(int l = 0, int? r = null)
    {
        if (_readyVideosList.Count == 0)
            return _readyVideosList;
        try
        {
            if (r == null)
                r = _readyVideosList.Count - 1;
            var i = l;
            var j = (int)r;
            var pivot = _readyVideosList[l].publishedAt;

            while (i <= j)
            {
                while (_readyVideosList[i].publishedAt > pivot)
                {
                    i++;
                }

                while (_readyVideosList[j].publishedAt < pivot)
                {
                    j--;
                }

                if (i <= j)
                {
                    var temp = _readyVideosList[i];
                    _readyVideosList[i] = _readyVideosList[j];
                    _readyVideosList[j] = temp;
                    i++;
                    j--;
                }
            }

            if (l < j)
                QuickSort(l, j);
            if (i < r)
                QuickSort(i, r);
            return _readyVideosList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return _readyVideosList;
        }
    }

    /// <summary>
    /// Вытягивание информации через API
    /// </summary>
    /// <param name="tempVideos">Список видео, для которых требуется дополнить информацию</param>
    /// <returns>Статус выполнения операции</returns>
    private async Task<bool> AppendInfoToVideos(List<dynamic> tempVideos)
    {
        try
        {
            string url = CreateYtGetVideosUrl(tempVideos);
            var response = await _httpClient.GetAsync(url);
            //Task message = Task.Run(() => Console.WriteLine(tempVideos.Count));
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent)!;
            List<Task> tasks = new List<Task>();
            foreach (dynamic responseItem in responseJson.items)
            {
                Task task = Task.Run(() =>
                {
                    AppendInfoToVideo(tempVideos, responseItem);
                });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            lock (_readyVideosListAddRangeLocker)
            {
                _readyVideosList.AddRange(tempVideos);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    /// Формирование ссылки для запроса к YouTube Api
    /// </summary>
    /// <param name="tempVideos">Список видео для дополнения информации</param>
    /// <returns>Ссылка запроса к YouTube Api</returns>
    private string CreateYtGetVideosUrl(List<dynamic> tempVideos)
    {
        var videosIds = new StringBuilder();
        foreach (var vid in tempVideos)
        {
            try
            {
                if (vid != null)
                    videosIds.Append("id=" + vid.id + "&");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        string url = "https://content-youtube.googleapis.com/youtube/v3/videos?";
        url += videosIds.ToString();
        url += $"part=snippet&prettyPrint=true&key={_youtubeKey}";
        return url;
    }

    /// <summary>
    /// Дополнение информации для видео из списка
    /// </summary>
    /// <param name="tempVideos">Список видео для дополнения</param>
    /// <param name="responseItem">Информация о видео с YouTube Api</param>
    private void AppendInfoToVideo(List<dynamic> tempVideos, dynamic responseItem)
    {
        try
        {
            string responseItemId = responseItem.id;
            var tempVideo = tempVideos.FirstOrDefault(v => v.id == responseItemId);
            if (tempVideo != null)
            {
                tempVideo.title = responseItem.snippet.title;
                tempVideo.channelTitle = responseItem.snippet.channelTitle;
                tempVideo.publishedAt = responseItem.snippet.publishedAt;
                int maxThumbnail = 0;
                foreach (var tn in responseItem.snippet.thumbnails) { maxThumbnail++; }
                tempVideo.maxThumbnail = maxThumbnail;
            }
            else
                Console.WriteLine(responseItemId + " не найден!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine(tempVideos);
        }
    }
}
