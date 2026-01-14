using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;

namespace OsuKeyPresses.Analysers;

internal class OsuKeyPressAnalyser(Score score) : KeyPressAnalyser(score)
{
    protected override int KeyCount => 2;

    protected override int[] GetActiveKeys(ReplayFrame frame)
        => ((OsuReplayFrame)frame).Actions
            .Select(x => (int)x)
            .Order()
            .ToArray();
}