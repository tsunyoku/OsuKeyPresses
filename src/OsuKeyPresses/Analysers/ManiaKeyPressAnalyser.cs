using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;

namespace OsuKeyPresses.Analysers;

internal class ManiaKeyPressAnalyser(Score score) : KeyPressAnalyser(score)
{
    protected override int KeyCount => 18;

    protected override int[] GetActiveKeys(ReplayFrame frame)
        => ((ManiaReplayFrame)frame).Actions
            .Select(x => (int)x)
            .Order()
            .ToArray();
}