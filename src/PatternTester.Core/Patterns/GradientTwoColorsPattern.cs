using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// A linear gradient between two arbitrary colors.
/// </summary>
public sealed class GradientTwoColorsPattern : PatternBase
{
    public GradientTwoColorsPattern()
        : base("Two Colors Gradient", "pattern_two_colors_gradient.png")
    {
    }

    public RgbColor StartColor { get; set; } = RgbColor.Red;

    public RgbColor EndColor { get; set; } = RgbColor.Blue;

    public PatternDirection Direction { get; set; } = PatternDirection.FromLeft;

    public override void Render(
        IPatternRenderContext c,
        PatternArea a,
        int cellNumber)
    {
        // StartColor/EndColor are swapped for FromRight/FromBottom so
        // that "StartColor" always appears first when reading the
        // gradient in the direction the user actually chose, rather
        // than always being anchored to a fixed screen edge regardless
        // of Direction. Without this swap, picking FromRight would
        // silently reverse which color reads as "the start" from the
        // user's point of view.
        var start = Direction is PatternDirection.FromRight
            or PatternDirection.FromBottom
            ? EndColor
            : StartColor;

        var end = Direction is PatternDirection.FromRight
            or PatternDirection.FromBottom
            ? StartColor
            : EndColor;

        c.DrawGradient(a, start, end, Direction);
    }
}
