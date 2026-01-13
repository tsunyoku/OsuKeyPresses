using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring.Legacy;

namespace ManiaKeyPresses;

internal class LocalLegacyScoreDecoder(BeatmapStore beatmapStore) : LegacyScoreDecoder
{
    private static readonly ManiaRuleset Mania = new();
    private static readonly TaikoRuleset Taiko = new();

    protected override Ruleset GetRuleset(int rulesetId)
    {
        return rulesetId switch
        {
            1 => Taiko,
            3 => Mania,
            _ => throw new InvalidOperationException("Tried to decode unsupported replay")
        };
    }

    // TODO: move the beatmap store call out of here, this is weird
    protected override WorkingBeatmap GetBeatmap(string md5Hash)
    {
        var osuFile = beatmapStore.GetBeatmapOsuFile(md5Hash);

        using var stream = new MemoryStream(osuFile);

        return new SlimWorkingBeatmap(stream);
    }
}