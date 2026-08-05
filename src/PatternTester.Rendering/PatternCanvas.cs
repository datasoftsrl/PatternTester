using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PatternTester.Core.Patterns;
using PatternTester.Core.Models;

namespace PatternTester.Rendering;

/// <summary>
/// The Avalonia control that actually puts pixels on screen. Its whole
/// job is turning "one pattern + a grid size" into "call Render once per
/// cell, in the right rectangle" — the pattern itself has no idea it's
/// being tiled into a grid at all (see <see cref="PatternBase.Render"/>).
/// Hosted fullscreen by PatternWindow in PatternTester.App.
/// </summary>
public sealed class PatternCanvas : Control
{
    public PatternCatalog? Catalog { get; set; }
    public int CurrentPatternIndex { get; set; }
    public int HorizontalScreens { get; set; } = 3;
    public int VerticalScreens { get; set; } = 3;

    /// <summary>Forces a redraw; call after changing any of the properties above, since Avalonia has no way to know they changed on their own.</summary>
    public void Refresh() => InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Catalog is null || Catalog.Items.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var pattern = Catalog[Math.Clamp(CurrentPatternIndex, 0, Catalog.Items.Count - 1)];
        var cols = Math.Max(1, HorizontalScreens);
        var rows = Math.Max(1, VerticalScreens);
        var cellWidth = Bounds.Width / cols;
        var cellHeight = Bounds.Height / rows;
        var renderer = new AvaloniaPatternRenderContext(context);

        for (var row = 0; row < rows; row++)
        for (var col = 0; col < cols; col++)
        {
            var area = new PatternArea(col * cellWidth, row * cellHeight, cellWidth, cellHeight);

            // 1-based, left-to-right then top-to-bottom — matches the
            // numbering shown by Geometry's on-screen cell label and by
            // "Identify Monitors", so the two stay consistent with each
            // other.
            var cellNumber = row * cols + col + 1;

            pattern.Render(renderer, area, cellNumber);

            // The red cell-boundary outline is drawn HERE, by the
            // canvas, rather than by the pattern itself — patterns that
            // opt in via DrawCellBorder only decide THAT a border is
            // wanted, not how it looks; keeping the actual drawing here
            // guarantees every bordered pattern gets a pixel-identical
            // border regardless of what the pattern's own Render does.
            if (pattern.DrawCellBorder)
            {
                context.DrawRectangle(
                    null,
                    new Pen(Brushes.Red, 1),
                    new Rect(area.X, area.Y, area.Width, area.Height));
            }
        }
    }
}
