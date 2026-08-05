using Xunit;
using PatternTester.Core.Models;

namespace PatternTester.Core.Tests;

public class PatternTests
{
    [Fact]
    public void PresetColorsRoundTrip()
    {
        Assert.Equal(RgbColor.Red, RgbColor.Parse("red"));
        Assert.Equal("red", RgbColor.Red.ToString());
        Assert.Equal(new RgbColor(12, 34, 56), RgbColor.Parse("12;34;56"));
    }

    [Fact]
    public void DirectionParsingIsStable()
    {
        Assert.Equal(PatternDirection.FromBottom, PatternDirectionExtensions.Parse("from_bottom"));
        Assert.Equal("from_right", PatternDirection.FromRight.ToConfigString());
    }
}
