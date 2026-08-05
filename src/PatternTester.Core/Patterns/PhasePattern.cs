using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// The finest possible alternating black/white line pattern (1 pixel
/// on, 1 pixel off) — historically the standard pattern for tuning the
/// sampling phase of an analog VGA connection, and on modern digital
/// displays repurposed as the sharpest possible stress test for
/// scaling/sampling fidelity (see docs/patterns/phase.md for the full
/// rationale). Only meaningful at the display's native resolution: any
/// scaling anywhere in the video path destroys the exact 1px alternation
/// this pattern depends on.
/// </summary>
public sealed class PhasePattern : PatternBase
{
    public PhasePattern() : base("Phase", "pattern_phase.png") { }
    public PatternDirection Direction { get; set; } = PatternDirection.FromBottom;

    public override void Render(IPatternRenderContext c, PatternArea a, int cellNumber)
    {
        c.Fill(a, RgbColor.Black);

        // Every white "line" is a 1px-wide (or 1px-tall) filled strip,
        // stepped by 2px so alternating strips are left as black
        // background — this is what produces the 1-on/1-off pattern.
        // FromLeft/FromRight both produce VERTICAL lines (stepping
        // across X); FromTop/FromBottom both produce HORIZONTAL lines
        // (stepping across Y). Direction here only controls the sweep
        // order (which edge the stepping starts from), which has no
        // visible effect on the final static pattern but keeps the
        // parameter consistent with how other patterns use Direction.
        if (Direction == PatternDirection.FromLeft)
        {
            for (var x = a.X; x < a.X + a.Width; x += 2)
                c.Fill(new PatternArea(x, a.Y, 1, a.Height), RgbColor.White);
        }
        else if (Direction == PatternDirection.FromRight)
        {
            for (var x = a.X + a.Width - 1; x >= a.X; x -= 2)
                c.Fill(new PatternArea(x, a.Y, 1, a.Height), RgbColor.White);
        }
        else if (Direction == PatternDirection.FromTop)
        {
            for (var y = a.Y; y < a.Y + a.Height; y += 2)
                c.Fill(new PatternArea(a.X, y, a.Width, 1), RgbColor.White);
        }
        else // FromBottom
        {
            for (var y = a.Y + a.Height - 1; y >= a.Y; y -= 2)
                c.Fill(new PatternArea(a.X, y, a.Width, 1), RgbColor.White);
        }
    }
}
