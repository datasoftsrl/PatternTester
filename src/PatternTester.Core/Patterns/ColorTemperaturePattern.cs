using PatternTester.Core.Models;
using PatternTester.Core.Services;

namespace PatternTester.Core.Patterns;

/// <summary>
/// Fills the screen with the RGB approximation of a given color
/// temperature (in Kelvin) — useful for judging a display's white
/// point / warm-vs-cool tint against a known reference.
/// </summary>
public sealed class ColorTemperaturePattern : PatternBase
{
    public ColorTemperaturePattern()
        : base("Color Temperature", "pattern_color_temperature.png")
    {
    }

    public int Temperature { get; set; } = 6500;

    public override void Render(
        IPatternRenderContext c,
        PatternArea a,
        int cellNumber)
    {
        c.Fill(a, KelvinToRgb(Temperature));
    }

    /// <summary>
    /// Converts a blackbody color temperature to an approximate sRGB
    /// color, using Tanner Helland's widely-used polynomial fit to
    /// Mitchell Charity's blackbody spectrum data (there is no simple
    /// closed-form solution for the true blackbody-to-sRGB conversion,
    /// so this curve-fit approximation is the standard practical choice
    /// — the same one used by many other display tools). The input is
    /// clamped to 2400–9500K, the range the fit remains reasonably
    /// accurate over and the range relevant for display white-point work.
    /// </summary>
    private static RgbColor KelvinToRgb(int kelvin)
    {
        // The fit's coefficients are calibrated for temperature/100,
        // not raw Kelvin.
        var temperature = Math.Clamp(kelvin, 2400, 9500) / 100.0;

        double red;
        double green;
        double blue;

        // Red: full intensity up to ~6600K, then falls off following
        // the fitted power curve above that.
        if (temperature <= 66)
        {
            red = 255;
        }
        else
        {
            red = 329.698727446 *
                  Math.Pow(temperature - 60, -0.1332047592);

            red = Math.Clamp(red, 0, 255);
        }

        // Green: two different fitted curves below/above ~6600K
        // (logarithmic below, power curve above), joined at that point.
        if (temperature <= 66)
        {
            green = 99.4708025861 *
                    Math.Log(temperature) - 161.1195681661;
        }
        else
        {
            green = 288.1221695283 *
                    Math.Pow(temperature - 60, -0.0755148492);
        }

        green = Math.Clamp(green, 0, 255);

        // Blue: full intensity from ~6600K up, zero below ~1900K, and a
        // fitted logarithmic curve in between.
        if (temperature >= 66)
        {
            blue = 255;
        }
        else if (temperature <= 19)
        {
            blue = 0;
        }
        else
        {
            blue = 138.5177312231 *
                   Math.Log(temperature - 10) - 305.0447927307;

            blue = Math.Clamp(blue, 0, 255);
        }

        return new RgbColor(
            (byte)Math.Round(red),
            (byte)Math.Round(green),
            (byte)Math.Round(blue));
    }
}
