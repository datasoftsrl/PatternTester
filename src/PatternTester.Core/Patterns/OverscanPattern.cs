using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// Draws two nested "safe area" guide rectangles — the classic
/// broadcast overscan check — so you can see exactly how much of the
/// edge of the image a display crops (overscans) versus what it shows
/// in full. If either guide is fully visible, that percentage of the
/// image survives on this display; if a guide is clipped, the display
/// is overscanning by at least that much.
/// </summary>
public sealed class OverscanPattern : PatternBase
{
    public OverscanPattern() : base("Overscan", "pattern_overscan.png") { }

    /// <summary>Outer guide, inset this % from each edge (broadcast "action-safe" convention is typically 10%).</summary>
    public double ActionSafePercent { get; set; } = 10;

    /// <summary>Inner guide, inset this % from each edge ("title-safe" convention is typically 20%).</summary>
    public double TitleSafePercent { get; set; } = 20;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        c.Fill(a, RgbColor.Black);

        DrawGuide(c, a, Math.Clamp(ActionSafePercent, 0, 45), RgbColor.White);
        DrawGuide(c, a, Math.Clamp(TitleSafePercent, 0, 45), RgbColor.Yellow);
    }

    /// <summary>
    /// IPatternRenderContext has no "draw rectangle outline" primitive
    /// on purpose — it only exposes the handful of primitives every
    /// existing pattern actually needed (see the interface's own
    /// summary). A rectangle outline is just four DrawLine calls, so
    /// there's no need to extend the interface for this pattern.
    /// </summary>
    private static void DrawGuide(IPatternRenderContext c, PatternArea a, double percent, RgbColor color)
    {
        var insetX = a.Width * percent / 100.0;
        var insetY = a.Height * percent / 100.0;

        var left = a.X + insetX;
        var top = a.Y + insetY;
        var right = a.X + a.Width - insetX;
        var bottom = a.Y + a.Height - insetY;

        c.DrawLine(left, top, right, top, color);
        c.DrawLine(right, top, right, bottom, color);
        c.DrawLine(right, bottom, left, bottom, color);
        c.DrawLine(left, bottom, left, top, color);
    }
}
