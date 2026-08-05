namespace PatternTester.Core.Models;

/// <summary>
/// A rectangular drawing region in device-independent pixels. Patterns
/// receive one of these per grid cell (see <see cref="PatternTester.Rendering.PatternCanvas"/>)
/// and draw entirely within it — a pattern never needs to know whether
/// it's rendering to the whole screen or to one cell of a multi-column/
/// row layout.
/// </summary>
public readonly record struct PatternArea(double X, double Y, double Width, double Height);
