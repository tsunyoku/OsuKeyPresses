using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Scoring;

namespace ManiaKeyPresses;

public class TaikoKeyPressAnalyser(Score score) : KeyPressAnalyser(score)
{
    protected override int KeyCount => 4;

    protected override int[] GetActiveKeys(ReplayFrame frame)
        => ((TaikoReplayFrame)frame).Actions
            .Select(x => (int)x)
            .Order()
            .ToArray();
}