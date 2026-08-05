using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// Gamma test via an alternating black/gray checkerboard (same technique
/// as <see cref="ChessboardPattern"/>, at a computed gray value instead
/// of white). The perceptual trick: at a normal viewing distance the eye
/// optically blends adjacent small black and gray squares into a single
/// averaged gray. If the display's actual gamma response matches the
/// <see cref="Value"/> this pattern was generated for, that blended
/// average should read as a neutral mid-gray; if the display's real
/// gamma is off, the blend will look too light or too dark, revealing
/// the mismatch without needing any measurement equipment.
/// </summary>
public sealed class GammaPattern : PatternBase
{
    public GammaPattern() : base("Gamma", "pattern_gamma.png") { }

    public double Value { get; set; } = 2.2;

    public int Cells { get; set; } = 16;

    public override void Render(
        IPatternRenderContext c,
        PatternArea a,
        int cellNumber)
    {
        var gamma = Math.Clamp(Value, 1.0, 3.5);
        var cells = Math.Clamp(Cells, 4, 128);

        var cellWidth = a.Width / cells;
        var cellHeight = a.Height / cells;

        // The light square's luminance is deliberately NOT 50% gray —
        // it's the gray value that, once passed back through the
        // display's assumed gamma curve, averages with pure black
        // (0%) to produce 50% perceived luminance. That's why gamma
        // 2.2 (the typical target) works out to roughly 50% raw pixel
        // value here rather than exactly 50%: lower gamma pulls the
        // curve up (a darker raw value is needed to still average to
        // 50%), higher gamma pushes it down (a lighter raw value is
        // needed).
        var normalized = Math.Clamp(
            Math.Pow(0.5, 1.0 / gamma),
            0.0,
            1.0);

        var value = (byte)Math.Clamp(
            Math.Round(normalized * 255.0),
            0,
            255);

        var light = new RgbColor(value, value, value);

        for (var row = 0; row < cells; row++)
        {
            for (var column = 0; column < cells; column++)
            {
                var color =
                    (row + column) % 2 == 0
                        ? light
                        : RgbColor.Black;

                var cell = new PatternArea(
                    a.X + column * cellWidth,
                    a.Y + row * cellHeight,
                    cellWidth,
                    cellHeight);

                c.Fill(cell, color);
            }
        }
    }
}
