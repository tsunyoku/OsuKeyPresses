using osu.Game.Scoring;

namespace ManiaKeyPresses.Helpers;

public static class ReplayHelper
{
    public static Score DecodeFromFile(
        string osrFile,
        string osuClientId,
        string osuClientSecret,
        string? beatmapCacheDirectory = null)
    {
        var oauthStore = new OAuthStore(osuClientId, osuClientSecret);
        var beatmapStore = new BeatmapStore(oauthStore, beatmapCacheDirectory);
        var scoreDecoder = new LocalLegacyScoreDecoder(beatmapStore);
        
        using var stream = File.OpenRead(osrFile);
        
        return scoreDecoder.Parse(stream);
    }
}