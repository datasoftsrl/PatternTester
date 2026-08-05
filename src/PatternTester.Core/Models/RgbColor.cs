namespace PatternTester.Core.Models;

/// <summary>
/// A plain 24-bit RGB color, independent of any UI framework's color
/// type (Avalonia's <c>Color</c> in particular) so that <c>PatternTester.Core</c>
/// stays free of UI dependencies. <see cref="PatternTester.Rendering.AvaloniaPatternRenderContext"/>
/// converts to/from Avalonia's own color type at the rendering boundary.
/// </summary>
public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static readonly RgbColor Black = new(0, 0, 0);
    public static readonly RgbColor White = new(255, 255, 255);
    public static readonly RgbColor Red = new(255, 0, 0);
    public static readonly RgbColor Green = new(0, 255, 0);
    public static readonly RgbColor Blue = new(0, 0, 255);
    public static readonly RgbColor Magenta = new(255, 0, 255);
    public static readonly RgbColor Yellow = new(255, 255, 0);
    public static readonly RgbColor Cyan = new(0, 255, 255);

    /// <summary>
    /// Parses either a named color ("black", "red", ...) as used in the
    /// JSON configuration file, or a raw "R;G;B" triplet for arbitrary
    /// custom colors picked by the user. Named colors round-trip through
    /// <see cref="ToString"/> back to their name (not the numeric
    /// triplet), which is what keeps the config file human-readable for
    /// the common case.
    /// </summary>
    public static RgbColor Parse(string? value, RgbColor fallback = default)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "black" => Black,
            "white" => White,
            "red" => Red,
            "green" => Green,
            "blue" => Blue,
            "magenta" => Magenta,
            "yellow" => Yellow,
            "cyan" => Cyan,
            _ => ParseRgb(value, fallback)
        };
    }

    private static RgbColor ParseRgb(string value, RgbColor fallback)
    {
        var parts = value.Split(';');
        if (parts.Length != 3) return fallback;
        return byte.TryParse(parts[0], out var r) && byte.TryParse(parts[1], out var g) && byte.TryParse(parts[2], out var b)
            ? new RgbColor(r, g, b)
            : fallback;
    }

    public override string ToString() => this switch
    {
        var c when c == Black => "black",
        var c when c == White => "white",
        var c when c == Red => "red",
        var c when c == Green => "green",
        var c when c == Blue => "blue",
        var c when c == Magenta => "magenta",
        var c when c == Yellow => "yellow",
        var c when c == Cyan => "cyan",
        _ => $"{R};{G};{B}"
    };
}
