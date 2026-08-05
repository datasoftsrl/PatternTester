using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// Draws a configurable number of bars, each a progressively dimmer
/// version of <see cref="Color"/> — a simple single-hue brightness step
/// wedge (see docs/patterns/bars.md for the display-testing rationale).
/// </summary>
public sealed class BarsPattern : PatternBase
{
    public BarsPattern()
        : base("Bars", "pattern_bars.png")
    {
    }

    public RgbColor Color { get; set; } = RgbColor.White;
    public PatternDirection Direction { get; set; } = PatternDirection.FromLeft;
    public int Number { get; set; } = 8;
    public int MaxNumber { get; set; } = 32;

    public override void Render(
        IPatternRenderContext c,
        PatternArea a,
        int cellNumber)
    {
        var bars = Math.Clamp(Number, 1, MaxNumber);

        for (var i = 0; i < bars; i++)
        {
            // The loop always walks the bars in on-screen left-to-right
            // (or top-to-bottom) order for the geometry below; "index"
            // is the bar's position in the BRIGHTNESS sequence instead,
            // which is reversed for FromRight/FromBottom so the
            // brightest bar always ends up adjacent to the chosen
            // starting edge regardless of which edge that is.
            var index =
                Direction is PatternDirection.FromRight
                    or PatternDirection.FromBottom
                    ? bars - 1 - i
                    : i;

            var fraction0 = index / (double)bars;
            var fraction1 = (index + 1) / (double)bars;

            // colorFactor turns "brightness rank" into an actual
            // multiplier: rank 0 (brightest) -> 1.0 (full color),
            // rank (bars-1) (dimmest) -> 0.0 (black), evenly spaced in
            // between. Guarded separately for bars == 1 to avoid a
            // divide-by-zero on (bars - 1).
            var colorFactor = bars == 1
                ? 1.0
                : (bars - 1 - index) / (double)(bars - 1);

            var color = Scale(Color, colorFactor);

            PatternArea r = Direction switch
            {
                PatternDirection.FromTop or PatternDirection.FromBottom =>
                    new PatternArea(
                        a.X,
                        a.Y + a.Height * fraction0,
                        a.Width,
                        a.Height * (fraction1 - fraction0)),

                PatternDirection.FromRight =>
                    new PatternArea(
                        a.X + a.Width * (1.0 - fraction1),
                        a.Y,
                        a.Width * (fraction1 - fraction0),
                        a.Height),

                _ =>
                    new PatternArea(
                        a.X + a.Width * fraction0,
                        a.Y,
                        a.Width * (fraction1 - fraction0),
                        a.Height)
            };

            c.Fill(r, color);
        }
    }

    /// <summary>Scales each channel of <paramref name="c"/> by <paramref name="factor"/> (0 = black, 1 = unchanged), rounding to the nearest byte.</summary>
    private static RgbColor Scale(RgbColor c, double factor) =>
        new(
            (byte)Math.Clamp(Math.Round(c.R * factor), 0, 255),
            (byte)Math.Clamp(Math.Round(c.G * factor), 0, 255),
            (byte)Math.Clamp(Math.Round(c.B * factor), 0, 255));
}
