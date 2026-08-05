using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// A linear gradient from <see cref="Color"/> down to black, used to
/// check for banding (visible discrete steps instead of a smooth
/// transition — see docs/patterns/gradients.md) in the display's
/// low-luminance rendering specifically, which is where banding is
/// usually most visible.
/// </summary>
public sealed class GradientToBlackPattern : PatternBase
{
    public GradientToBlackPattern()
        : base("To Black Gradient", "pattern_to_black_gradient.png")
    {
    }

    public RgbColor Color { get; set; } = RgbColor.White;

    public PatternDirection Direction { get; set; } = PatternDirection.FromLeft;

    public override void Render(
        IPatternRenderContext c,
        PatternArea a,
        int cellNumber)
    {
        c.DrawGradient(
            a,
            Color,
            RgbColor.Black,
            Direction);
    }
}
