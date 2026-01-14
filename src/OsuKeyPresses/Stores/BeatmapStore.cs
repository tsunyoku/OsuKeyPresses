using System.Net.Http.Headers;
using System.Text.Json;
using OsuKeyPresses.Models;

namespace OsuKeyPresses.Stores;

internal class BeatmapStore
{
    private readonly OAuthStore _oauthStore;
    private readonly string _beatmapCache;

    public BeatmapStore(OAuthStore oauthStore, string? beatmapCacheDirectory = null)
    {
        _oauthStore = oauthStore;
        _beatmapCache = beatmapCacheDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "beatmaps");

        if (!Directory.Exists(_beatmapCache))
            Directory.CreateDirectory(_beatmapCache);
    }

    public byte[] GetBeatmapOsuFile(string checksum)
    {
        var beatmapPath = Path.Combine(_beatmapCache, checksum);
        
        if (File.Exists(beatmapPath))
            return File.ReadAllBytes(beatmapPath);

        var beatmapId = GetBeatmapId(checksum);

        var osuFile = GetOsuFile(beatmapId);
        
        File.WriteAllBytes(beatmapPath, osuFile);

        return osuFile;
    }

    private static byte[] GetOsuFile(int beatmapId)
    {
        using var httpClient = new HttpClient();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/osu/{beatmapId}");

        var response = httpClient.Send(request);

        response.EnsureSuccessStatusCode();
        
        using var stream = response.Content.ReadAsStream();
        using var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    private int GetBeatmapId(string checksum)
    {
        var accessToken = _oauthStore.GetAccessToken();
        
        using var httpClient = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/beatmaps/lookup?checksum={checksum}");
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = httpClient.Send(request);

        response.EnsureSuccessStatusCode();
        
        using var jsonResponse = response.Content.ReadAsStream();
        var beatmap = JsonSerializer.Deserialize<Beatmap>(jsonResponse);
        
        return beatmap!.Id;
    }
}