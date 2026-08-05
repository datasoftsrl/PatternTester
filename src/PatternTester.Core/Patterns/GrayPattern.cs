using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// A flat gray field at an adjustable brightness percentage — see
/// <see cref="SingleColorPattern"/> for the general flat-field use case;
/// this variant exists specifically so a precise, arbitrary gray level
/// (rather than only pure black/white/primaries) can be dialed in.
/// </summary>
public sealed class GrayPattern : PatternBase
{
    public GrayPattern() : base("Gray", "pattern_gray.png") { }

    /// <summary>Brightness as a 0–100 percentage of full white, as shown to the user (kept in this unit rather than raw 0–255 so the UI/config stay resolution- and encoding-independent).</summary>
    public double White { get; set; } = 50;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        // 2.55 = 255 / 100: converts the 0-100 percentage into a 0-255
        // byte value.
        var v = (byte)Math.Clamp(Math.Round(White * 2.55), 0, 255);
        c.Fill(a, new RgbColor(v, v, v));
    }
}
