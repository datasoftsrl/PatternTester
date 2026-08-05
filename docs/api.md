# API Reference

PatternTester is a desktop application, not a networked service — there is no REST/HTTP API. This page documents the public surface of **`PatternTester.Core`**, the platform-independent library that defines what a test pattern is and how it is rendered. It has no dependency on Avalonia or any UI framework, so it can be reused or unit-tested in isolation (see `tests/PatternTester.Core.Tests`).

---

## `PatternBase`

Abstract base class every pattern derives from.

```csharp
public abstract class PatternBase
{
    protected PatternBase(string name, string? iconName = null);

    public string Name { get; }
    public string? IconName { get; }

    // If true, the rendering engine draws a red border around each
    // cell after the pattern itself has been rendered.
    public virtual bool DrawCellBorder => false;

    public abstract void Render(IPatternRenderContext context, PatternArea area, int cellNumber);
}
```

`Render` is called once per grid cell (see [Columns and Rows](user.md#columns-and-rows-split-screen-patterns) in the User Guide) with the destination `PatternArea` and the zero-based `cellNumber`, so a pattern can vary its output across cells if needed (most patterns render the same content in every cell).

## `PatternCatalog`

Holds the ordered list of every built-in pattern, in the same order they appear in the **Pattern** menu.

```csharp
public sealed class PatternCatalog
{
    public IReadOnlyList<PatternBase> Items { get; }
    public PatternBase this[int index] { get; }
}
```

## Built-in Patterns

All under `PatternTester.Core.Patterns`, each a concrete `PatternBase`:

| Class | Menu name |
|---|---|
| `GeometryPattern` | Geometry |
| `SingleColorPattern` | Single Color |
| `GrayPattern` | Gray |
| `GammaPattern` | Gamma |
| `BarsPattern` | Bars |
| `ColorBarsPattern` | Color Bars |
| `GradientToBlackPattern` | Gradient to Black |
| `GradientTwoColorsPattern` | Two-Color Gradient |
| `ChessboardPattern` | Chessboard |
| `PhasePattern` | Phase |
| `ColorTemperaturePattern` | Color Temperature |

See [Pattern Reference](patterns.md) for what each one is used for from a user's perspective.

## Adding a New Pattern

1. Create a new class under `PatternTester.Core/Patterns/` deriving from `PatternBase`.
2. Implement `Render(...)`, using the `IPatternRenderContext` passed in to draw.
3. Register it in `PatternCatalog`'s constructor, in the position you want it to appear in the **Pattern** menu.
4. Add a `Pattern.<Name>` string to both `Languages/it.json` and `Languages/en.json` in `PatternTester.App`.
5. If the pattern has configurable parameters, add the corresponding controls to the settings panel in `MainWindow.axaml` (bound to a new property on `MainWindowViewModel`), following the pattern used by the existing ones (e.g. Gamma's numeric value, Chessboard's grid size).

## `PatternTester.Rendering`

`PatternCanvas` (in `PatternTester.Rendering`) is the Avalonia control that hosts a `PatternBase` and turns its `Render` calls into actual pixels on screen, handling the column/row grid layout and the optional cell border. It is the one piece of `Rendering` that does depend on Avalonia — `Core` itself stays UI-framework-agnostic.

---

## Implementation Notes

The algorithms below are not obvious from the pattern's *name* or from looking at its output alone — each one encodes a specific technique worth understanding before touching the code. Full inline comments live next to each implementation (`src/PatternTester.Core/Patterns/`); this section is the "why", collected in one place.

### Gamma — the checkerboard trick

`GammaPattern` doesn't measure anything — it exploits the eye's own optical averaging. At normal viewing distance, small adjacent black and gray squares blend into a single perceived gray. The gray square's raw pixel value is deliberately **not** 50%:

```
normalized = 0.5 ^ (1 / gamma)
```

This is the inverse of the standard gamma encoding formula (`output = input ^ gamma`), solved for the raw value that, once the display applies *its own* gamma response, should visually average with pure black to exactly 50% perceived brightness — but only if the display's actual gamma matches the value the pattern was generated for. A display with the wrong gamma will make the blended checkerboard look too light or too dark, without needing a colorimeter to see it.

### Bars — brightness ranking vs. screen position

`BarsPattern` separates two ideas that are easy to conflate: **loop index** `i` (always walks left-to-right / top-to-bottom on screen) and **brightness rank** `index` (which bar in the light→dark sequence occupies that screen position). They're only the same value for `FromLeft`/`FromTop`; for `FromRight`/`FromBottom` the rank is reversed (`bars - 1 - i`) so the brightest bar always ends up adjacent to whichever edge was chosen as the start — the geometry code doesn't need its own branch per direction, only the rank calculation does. The brightness multiplier itself is a simple linear ramp:

```
colorFactor = (bars - 1 - index) / (bars - 1)     // rank 0 → 1.0 (full color), rank (bars-1) → 0.0 (black)
```

### Color Bars — overlapping rectangles, not adjacent ones

`ColorBarsPattern`'s band geometry looks like a bug on first read: every band's rectangle extends all the way to the *far* edge of the area, not just to the start of the next band. That's intentional — bands are drawn in sequence (black first, white last), each one painting over the tail end of the previous, wider rectangle. Only each band's own leading sliver survives being overpainted by every subsequent band, and the net visual result is 8 equal, non-overlapping bands, reached by successive overpainting rather than by computing 8 independent, mutually-exclusive rectangles.

### Phase — 1px-on/1px-off, and why direction barely matters

`PhasePattern` fills 1-pixel-wide (or tall) white strips every 2 pixels — the finest alternating detail a raster display can be asked to show, which is exactly what makes it useful (see [Phase](patterns/phase.md) for the historical VGA-phase-tuning rationale). Unlike every other directional pattern, `Direction` here only changes the axis (`FromLeft`/`FromRight` → vertical lines, `FromTop`/`FromBottom` → horizontal lines) and the sweep order the strips are drawn in — since the pattern is perfectly periodic, sweep order has no visible effect on the final static image; the parameter is kept mainly for consistency with how every other pattern uses `Direction`.

### Color Temperature — Kelvin to RGB

`ColorTemperaturePattern` converts a Kelvin value to sRGB using Tanner Helland's polynomial fit to Mitchell Charity's blackbody spectrum measurements. There is no simple closed-form solution for true blackbody-to-sRGB conversion, which is why a curve-fit approximation is standard practice here (the same fit is used by a number of other display and lighting tools). Each channel uses its own fitted curve, split at ~6600K:

- **Red** — full intensity below ~6600K, a fitted power-law falloff above it.
- **Green** — a fitted logarithmic curve below ~6600K, a different fitted power-law curve above it, joined at that point.
- **Blue** — zero below ~1900K, full intensity above ~6600K, a fitted logarithmic curve in between.

Input is clamped to 2400–9500K, the range the fit stays reasonably accurate over and the range relevant to display white-point work.
