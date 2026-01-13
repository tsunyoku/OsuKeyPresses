using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Game.Utils;

namespace ManiaKeyPresses;

public class ManiaKeyPressAnalyser : KeyPressAnalyser
{
    public ManiaKeyPressAnalyser(string osrFile, string osuClientId, string osuClientSecret, string? beatmapCacheDirectory = null)
        : base(osrFile, osuClientId, osuClientSecret, beatmapCacheDirectory)
    {
    }

    protected override int KeyCount => 18;

    protected override int[] GetActiveKeys(ReplayFrame frame)
        => ((ManiaReplayFrame)frame).Actions
            .Select(x => (int)x)
            .Order()
            .ToArray();
}