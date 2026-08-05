namespace PatternTester.Core.Patterns;

/// <summary>
/// The full list of built-in patterns, in construction order. That
/// order is not cosmetic: it is exactly the order the Pattern menu
/// shows them in, and <see cref="MainWindowViewModel.SelectedPatternIndex"/>
/// (in PatternTester.App) is a plain index into <see cref="Items"/> —
/// so inserting a pattern in the middle of this list, rather than
/// appending it, changes what a previously-saved index points to for
/// anyone with an existing configuration file. Add new patterns at the
/// end unless you're deliberately fine with that.
/// </summary>
public sealed class PatternCatalog
{
    public PatternCatalog()
    {
        Items =
        [
            new GeometryPattern(),
            new SingleColorPattern(),
            new GrayPattern(),
            new GammaPattern(),
            new BarsPattern(),
            new ColorBarsPattern(),
            new GradientToBlackPattern(),
            new GradientTwoColorsPattern(),
            new ChessboardPattern(),
            new PhasePattern(),
            new ColorTemperaturePattern(),
            new OverscanPattern()
           
        ];
    }

    public IReadOnlyList<PatternBase> Items { get; }
    public PatternBase this[int index] => Items[index];
}
