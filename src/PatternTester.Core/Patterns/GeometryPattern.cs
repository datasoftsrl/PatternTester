using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// A grid of horizontal/vertical (and optionally diagonal) lines plus a
/// centered circle, used to check geometric linearity, aspect ratio and
/// overscan/underscan. Also draws the cell number in the middle — see
/// <see cref="DrawCellBorder"/> — which, combined with the red cell
/// border every pattern with that flag gets, makes it easy to confirm
/// which physical screen/cell is which when several are shown at once.
/// </summary>
public sealed class GeometryPattern : PatternBase
{
    public GeometryPattern() : base("Geometry Pattern", "pattern_geometry.png") { }
    public int Lines { get; set; } = 8;
    public bool DiagonalLines { get; set; } = true;
    public bool Circle { get; set; } = true;
    public int MaxLines { get; set; } = 16;

    /// <summary>Geometry relies on the red cell-boundary outline (drawn by PatternCanvas) to give the grid a definite outer edge to be checked against.</summary>
    public override bool DrawCellBorder => true;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        c.Fill(a, RgbColor.Black);
        var count = Math.Max(0, Lines);

        // +1 in the denominator (and looping through i == count + 1)
        // is what places lines evenly INSIDE the area including its
        // own edges: with N internal divisions you need N+1 gaps, and
        // the loop deliberately draws one extra line so the area's
        // own boundary is covered too, not just the internal grid.
        var denominator = count + 1.0;
        for (var i = 0; i <= count + 1; i++)
        {
            var dx = i * a.Width / denominator;
            var dy = i * a.Height / denominator;
            c.DrawLine(a.X, a.Y + dy, a.X + a.Width, a.Y + dy, RgbColor.White);
            c.DrawLine(a.X + dx, a.Y, a.X + dx, a.Y + a.Height, RgbColor.White);

            if (DiagonalLines)
            {
                // Four diagonals per step, one per corner, each running
                // from a point on the current horizontal grid line to
                // the mirrored point on the current vertical grid line
                // — together they build up the classic crossed-diagonal
                // "starburst" look as i sweeps from 0 to count+1, rather
                // than drawing just the two full corner-to-corner
                // diagonals.
                c.DrawLine(a.X, a.Y + dy, a.X + dx, a.Y, RgbColor.White);
                c.DrawLine(a.X + a.Width - dx, a.Y + a.Height, a.X + a.Width, a.Y + a.Height - dy, RgbColor.White);
                c.DrawLine(a.X + a.Width, a.Y + dy, a.X + a.Width - dx, a.Y, RgbColor.White);
                c.DrawLine(a.X, a.Y + a.Height - dy, a.X + dx, a.Y + a.Height, RgbColor.White);
            }
        }

        if (Circle)
        {
            // Diameter is the smaller of width/height so the circle
            // always fits inside the area regardless of aspect ratio —
            // on a non-square cell this deliberately makes it an
            // inscribed circle, not a true full-bleed ellipse, since a
            // stretched ellipse would defeat the point of using a
            // circle to check for aspect-ratio distortion in the first
            // place.
            var size = Math.Min(a.Width, a.Height);
            c.DrawEllipse(new PatternArea(a.X + (a.Width - size) / 2, a.Y + (a.Height - size) / 2, size, size), RgbColor.White);
        }

        c.DrawTextCentered(a, cellNumber.ToString(), RgbColor.Cyan, 100);
    }
}
