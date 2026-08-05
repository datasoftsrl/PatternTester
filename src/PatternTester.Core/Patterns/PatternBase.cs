using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// Base class every test pattern derives from. A pattern is deliberately
/// "dumb": it knows nothing about monitors, windows, or how many cells
/// the screen is split into — it just draws itself into whatever
/// rectangle it's handed. That separation is what lets the same pattern
/// code run identically whether it fills one fullscreen monitor or one
/// cell of a 3x3 grid split across several screens.
/// </summary>
public abstract class PatternBase
{
    protected PatternBase(string name, string? iconName = null)
    {
        Name = name;
        IconName = iconName;
    }

    /// <summary>Internal identifier used to key into localized strings ("Pattern.&lt;Name&gt;") — NOT the display name shown to the user.</summary>
    public string Name { get; }

    /// <summary>File name of the preview thumbnail under App/Assets/Patterns, or null if the pattern has none.</summary>
    public string? IconName { get; }

    /// <summary>
    /// Indicates whether the rendering engine should draw a red border
    /// around each cell after the pattern has been rendered. Used by
    /// Geometry, where the border marks the exact cell boundary that the
    /// pattern's own lines/circle are meant to be checked against.
    /// </summary>
    public virtual bool DrawCellBorder => false;

    /// <summary>
    /// Draws the pattern into <paramref name="area"/>. Called once per
    /// grid cell by <see cref="PatternTester.Rendering.PatternCanvas"/>;
    /// <paramref name="cellNumber"/> is the 1-based index of that cell
    /// (left-to-right, top-to-bottom), which most patterns ignore but
    /// Geometry uses to print a visible cell number for identifying
    /// individual screens in a multi-monitor grid.
    /// </summary>
    public abstract void Render(IPatternRenderContext context, PatternArea area, int cellNumber);
}
