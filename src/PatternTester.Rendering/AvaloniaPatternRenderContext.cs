using Avalonia;
using Avalonia.Media;
using PatternTester.Core.Models;
using PatternTester.Core.Services;
using System.Globalization;

namespace PatternTester.Rendering;

/// <summary>
/// The only implementation of <see cref="IPatternRenderContext"/>:
/// translates the framework-agnostic drawing calls patterns make into
/// Avalonia's own <c>DrawingContext</c> API. This is the sole place in
/// the codebase where <c>PatternTester.Core</c>'s drawing primitives
/// meet an actual UI framework — if PatternTester ever needed a second
/// rendering backend (a headless image exporter, say), this is the only
/// class that would need a counterpart.
/// </summary>
public sealed class AvaloniaPatternRenderContext : IPatternRenderContext
{
    private readonly DrawingContext _context;

    public AvaloniaPatternRenderContext(DrawingContext context) => _context = context;

    public void Fill(PatternArea area, RgbColor color) =>
        _context.FillRectangle(ToBrush(color), new Rect(area.X, area.Y, area.Width, area.Height));

    public void DrawLine(double x1, double y1, double x2, double y2, RgbColor color, double width = 1) =>
        _context.DrawLine(new Pen(ToBrush(color), width), new Point(x1, y1), new Point(x2, y2));

    public void DrawEllipse(PatternArea area, RgbColor color, double width = 1) =>
        _context.DrawEllipse(null, new Pen(ToBrush(color), width),
            new Point(area.X + area.Width / 2, area.Y + area.Height / 2), area.Width / 2, area.Height / 2);

    public void DrawGradient(PatternArea area, RgbColor start, RgbColor end, PatternDirection direction)
    {
        // (p1, p2) is the gradient's axis, expressed as absolute start/
        // end points rather than the 0-1 relative coordinates
        // LinearGradientBrush also supports — absolute is what lets the
        // same code work correctly for a non-square area without the
        // gradient direction being skewed by the area's aspect ratio.
        var (p1, p2) = direction switch
        {
            PatternDirection.FromTop =>
                (new RelativePoint(area.X, area.Y, RelativeUnit.Absolute),
                 new RelativePoint(area.X, area.Y + area.Height, RelativeUnit.Absolute)),
            PatternDirection.FromBottom =>
                (new RelativePoint(area.X, area.Y + area.Height, RelativeUnit.Absolute),
                 new RelativePoint(area.X, area.Y, RelativeUnit.Absolute)),
            PatternDirection.FromRight =>
                (new RelativePoint(area.X + area.Width, area.Y, RelativeUnit.Absolute),
                 new RelativePoint(area.X, area.Y, RelativeUnit.Absolute)),
            _ =>
                (new RelativePoint(area.X, area.Y, RelativeUnit.Absolute),
                 new RelativePoint(area.X + area.Width, area.Y, RelativeUnit.Absolute))
        };

        var brush = new LinearGradientBrush
        {
            StartPoint = p1,
            EndPoint = p2,
            GradientStops =
            [
                new GradientStop(ToColor(start), 0),
                new GradientStop(ToColor(end), 1)
            ]
        };
        _context.FillRectangle(brush, new Rect(area.X, area.Y, area.Width, area.Height));
    }

    public void DrawTextCentered(PatternArea area, string text, RgbColor color, double fontSize)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Sans"),
            fontSize,
            ToBrush(color));

        // FormattedText already knows its own rendered width/height, so
        // centering is just "area midpoint minus half the text's own
        // size" — no separate text-measurement pass needed.
        var x = area.X + (area.Width - formatted.Width) / 2;
        var y = area.Y + (area.Height - formatted.Height) / 2;
        _context.DrawText(formatted, new Point(x, y));
    }

    private static SolidColorBrush ToBrush(RgbColor color) => new(ToColor(color));

    /// <summary>Alpha is always 255: PatternTester.Core's RgbColor has no alpha channel — every pattern is fully opaque by design.</summary>
    private static Avalonia.Media.Color ToColor(RgbColor color) => new(255, color.R, color.G, color.B);
}
