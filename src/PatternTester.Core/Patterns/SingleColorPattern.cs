using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// The simplest pattern: fills the area with one flat <see cref="Color"/>.
/// Precisely because there's no gradient/geometry to distract the eye,
/// it's the most sensitive pattern for spotting dead/stuck pixels and
/// backlight/uniformity issues — see docs/patterns/single-color.md.
/// </summary>
public sealed class SingleColorPattern : PatternBase
{
    public SingleColorPattern() : base("Single Color", "pattern_single_color.png") { }

    public RgbColor Color { get; set; } = RgbColor.Black;

    /// <summary>UI hint only (not used by <see cref="Render"/> itself): true selects Color from a preset swatch list, false lets the user pick an arbitrary custom color.</summary>
    public bool UsePreset { get; set; } = true;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber) => c.Fill(a, Color);
}
