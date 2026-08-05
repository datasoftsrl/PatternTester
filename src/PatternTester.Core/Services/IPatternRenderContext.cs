using PatternTester.Core.Models;

namespace PatternTester.Core.Services;

/// <summary>
/// Minimal drawing surface a <see cref="PatternTester.Core.Patterns.PatternBase"/>
/// draws itself onto. Deliberately small and framework-agnostic: it only
/// exposes the handful of primitives every pattern actually needs (flat
/// fills, lines, an ellipse, a linear gradient, centered text), so that
/// <c>PatternTester.Core</c> never has to reference Avalonia (or any other
/// UI framework) directly. <see cref="PatternTester.Rendering.AvaloniaPatternRenderContext"/>
/// is the only implementation today, translating these calls into
/// Avalonia's <c>DrawingContext</c> API, but a pattern's <c>Render</c>
/// method has no idea that's happening on the other side of this interface.
/// </summary>
public interface IPatternRenderContext
{
    void Fill(PatternArea area, RgbColor color);
    void DrawLine(double x1, double y1, double x2, double y2, RgbColor color, double width = 1);
    void DrawEllipse(PatternArea area, RgbColor color, double width = 1);

    /// <summary>
    /// Draws a linear gradient across <paramref name="area"/> from
    /// <paramref name="start"/> to <paramref name="end"/>, oriented
    /// according to <paramref name="direction"/> (the gradient runs
    /// "away from" that edge — e.g. <see cref="PatternDirection.FromLeft"/>
    /// starts at the left edge and ends at the right).
    /// </summary>
    void DrawGradient(PatternArea area, RgbColor start, RgbColor end, PatternDirection direction);

    void DrawTextCentered(PatternArea area, string text, RgbColor color, double fontSize);
}
