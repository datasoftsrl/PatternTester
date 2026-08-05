namespace PatternTester.Core.Models;

/// <summary>
/// The edge a directional pattern (gradients, Bars, Color Bars, Phase)
/// treats as its "starting" side. For gradients this is where
/// <c>start</c>/<c>StartColor</c> is anchored; for Bars/Color Bars it's
/// where the first (brightest, or first-in-sequence) band begins; for
/// Phase it controls whether the alternating lines run vertically or
/// horizontally.
/// </summary>
public enum PatternDirection
{
    FromLeft,
    FromRight,
    FromTop,
    FromBottom
}

/// <summary>
/// Conversion between <see cref="PatternDirection"/> and the lowercase
/// snake_case strings used in the JSON configuration file, kept stable
/// independently of the C# enum member names so existing config files
/// don't break if the enum is ever refactored.
/// </summary>
public static class PatternDirectionExtensions
{
    public static PatternDirection Parse(string? value, PatternDirection fallback = PatternDirection.FromLeft) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "from_right" => PatternDirection.FromRight,
            "from_top" => PatternDirection.FromTop,
            "from_bottom" => PatternDirection.FromBottom,
            "from_left" => PatternDirection.FromLeft,
            _ => fallback
        };

    public static string ToConfigString(this PatternDirection value) => value switch
    {
        PatternDirection.FromRight => "from_right",
        PatternDirection.FromTop => "from_top",
        PatternDirection.FromBottom => "from_bottom",
        _ => "from_left"
    };
}
