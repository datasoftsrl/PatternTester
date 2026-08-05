using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// The classic 8-band television color bars (black, blue, red, magenta,
/// green, cyan, yellow, white — the standard order in which each
/// successive bar adds exactly one more RGB primary than the last).
/// Unlike <see cref="BarsPattern"/>, the band count is fixed at 8 (it's
/// a fixed reference pattern, not an adjustable step wedge) and every
/// band is a saturated primary/secondary rather than a brightness ramp.
/// </summary>
public sealed class ColorBarsPattern : PatternBase
{
    public ColorBarsPattern() : base("Colored Bars", "pattern_colored_bars.png") { }

    public PatternDirection Direction { get; set; } = PatternDirection.FromLeft;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        RgbColor[] colors =
        [
            RgbColor.Black,
            RgbColor.Blue,
            RgbColor.Red,
            RgbColor.Magenta,
            RgbColor.Green,
            RgbColor.Cyan,
            RgbColor.Yellow,
            RgbColor.White
        ];

        for (var i = 0; i < colors.Length; i++)
        {
            // Same "reverse the sequence, not the geometry" approach as
            // BarsPattern: i always walks colors[] in its natural
            // black-to-white order; "index" picks which entry of
            // colors[] lands in that visual slot, reversed for
            // FromRight/FromBottom so black stays adjacent to the
            // chosen starting edge.
            var index = Direction is PatternDirection.FromRight or PatternDirection.FromBottom
                ? colors.Length - 1 - i
                : i;

            // Non-obvious geometry: f1 is NOT a width fraction relative
            // to f0 (unlike BarsPattern's fraction0/fraction1 pair) —
            // it's used directly as the rectangle's width, and every
            // band's rectangle extends all the way to the far edge
            // (f0 + f1 always equals 1). Bands are drawn in order
            // i = 0..7, each one painting OVER the tail end of the
            // previous, wider rectangle; since band i+1's rectangle
            // starts further in and is drawn on top, only the leading
            // 1/8th sliver of each earlier band stays visible once all
            // 8 have been drawn. The net visual result is 8 equal,
            // non-overlapping bands — just reached by successive
            // right-aligned overpainting rather than by computing each
            // band's rectangle independently.
            var f0 = i / 8.0;
            var f1 = (8 - i) / 8.0;

            PatternArea r = Direction switch
            {
                PatternDirection.FromTop or PatternDirection.FromBottom =>
                    new PatternArea(
                        a.X,
                        a.Y + a.Height * f0,
                        a.Width,
                        a.Height * f1),

                _ =>
                    new PatternArea(
                        a.X + a.Width * f0,
                        a.Y,
                        a.Width * f1,
                        a.Height)
            };

            c.Fill(r, colors[index]);
        }
    }
}
