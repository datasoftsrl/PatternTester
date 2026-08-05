using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// A black/white checkerboard grid. Standalone as its own menu entry,
/// but also the visual technique <see cref="GammaPattern"/> reuses at a
/// fixed 2-color-alternation to make gamma perceptible at a glance —
/// see that class for why the alternating-square trick works.
/// </summary>
public sealed class ChessboardPattern : PatternBase
{
    public ChessboardPattern() : base("Chessboard", "pattern_chessboard.png") { }

    public int Squares { get; set; } = 10;
    public int MaxSquares { get; set; } = 16;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        var n = Math.Clamp(Squares, 2, 16);

        c.Fill(a, RgbColor.Black);

        // Only the "white" squares are drawn on top of the black fill
        // (standard checkerboard parity: x+y even); the black squares
        // are simply the fill showing through.
        for (var x = 0; x < n; x++)
        for (var y = 0; y < n; y++)
            if ((x + y) % 2 == 0)
                c.Fill(
                    new PatternArea(
                        a.X + x * a.Width / n,
                        a.Y + y * a.Height / n,
                        a.Width / n,
                        a.Height / n),
                    RgbColor.White);
    }
}
