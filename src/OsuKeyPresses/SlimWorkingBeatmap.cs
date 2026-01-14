using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Mania;
using osu.Game.Skinning;

namespace OsuKeyPresses;

internal class SlimWorkingBeatmap : WorkingBeatmap
{
    private static readonly ManiaRuleset Ruleset = new();

    private readonly Beatmap _underlyingBeatmap;
    
    public SlimWorkingBeatmap(Stream stream)
        : this(new LineBufferedReader(stream))
    {
        stream.Dispose();
    }

    private SlimWorkingBeatmap(LineBufferedReader reader)
        : this(Decoder.GetDecoder<Beatmap>(reader).Decode(reader))
    {
        reader.Dispose();
    }

    private SlimWorkingBeatmap(Beatmap underlyingBeatmap)
        : base(underlyingBeatmap.BeatmapInfo, null)
    {
        _underlyingBeatmap = underlyingBeatmap;
        _underlyingBeatmap.BeatmapInfo.Ruleset = Ruleset.RulesetInfo;
    }

    protected override IBeatmap GetBeatmap() => _underlyingBeatmap;
    public override Texture GetBackground() => throw new NotImplementedException();
    protected override Track GetBeatmapTrack() => throw new NotImplementedException();
    protected override ISkin GetSkin() => throw new NotImplementedException();
    public override Stream GetStream(string storagePath) => throw new NotImplementedException();
}