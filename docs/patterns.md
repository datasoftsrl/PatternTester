# Pattern Reference

This section explains the purpose and correct use of each test pattern, aiming to document not just **what** a pattern displays, but **why** it exists and **how** to interpret it.

!!! info "Two kinds of pages here"
    PatternTester currently implements **11 patterns**, listed under [Available Patterns](#available-patterns) below — each links to its real page. The rest of this section is a broader **display-testing reference library**: pages about topics PatternTester does not generate a dedicated pattern for yet (contrast, uniformity, HDR, color gamuts, motion, and so on). They are kept because the background knowledge is useful on its own, and because they double as a roadmap for patterns that could be added later — but don't expect the corresponding menu entry to exist today.

---

## Available Patterns

These correspond 1:1 to the entries in the **Pattern** menu, in the same order:

| Pattern | Page | What it checks |
|---|---|---|
| Geometry | [geometry.md](patterns/geometry.md) | Straight lines, proportions, overscan, centering |
| Single Color | [single-color.md](patterns/single-color.md) | Flat-field uniformity, dead/stuck pixels |
| Gray | [grayscale.md](patterns/grayscale.md) | Gray level / white percentage rendering |
| Gamma | [gamma.md](patterns/gamma.md) | Gamma response via a checkerboard at an adjustable value |
| Bars | [bars.md](patterns/bars.md) | Quick brightness step-separation check in a chosen color |
| Color Bars | [color-bars.md](patterns/color-bars.md) | Color reproduction accuracy |
| Gradient to Black | [gradients.md](patterns/gradients.md) | Banding/smoothness of a gradient fading to black |
| Two-Color Gradient | [gradients.md](patterns/gradients.md) | Banding/smoothness of a gradient between two colors |
| Chessboard | [checkerboard.md](patterns/checkerboard.md) | Grid uniformity, also used as the base for the Gamma pattern |
| Phase | [phase.md](patterns/phase.md) | Pixel-level sampling/scaling fidelity, phase/timing artifacts |
| Color Temperature | [color-temperature.md](patterns/color-temperature.md) | White point / color temperature |

For the configurable parameters of each pattern (e.g. gamma value, grid size, gradient direction), see the [User Guide](user.md).

---

## Reference Library (not all implemented yet)

Organized by category — every linked page exists and can be read today, independently of whether PatternTester has a matching menu entry.

**Geometry & Alignment**
[Aspect Ratio](patterns/aspect-ratio.md) ·
[Overscan](patterns/overscan.md) ·
[Grid Alignment](patterns/grid-alignment.md) ·
[Convergence](patterns/convergence.md) ·
[Pixel Mapping](patterns/pixel-mapping.md) ·
[Dot Pitch](patterns/dot-pitch.md)

**Sharpness & Focus**
[Sharpness](patterns/sharpness.md) ·
[Focus](patterns/focus.md) ·
[Siemens Star](patterns/siemens-star.md) ·
[Crosshatch](patterns/crosshatch.md) ·
[Moiré](patterns/moire.md) ·
[Zone Plate](patterns/zone-plate.md)

**Contrast & Levels**
[Contrast](patterns/contrast.md) ·
[Black Level](patterns/black-level.md) ·
[White Level](patterns/white-level.md) ·
[Local Dimming](patterns/local-dimming.md) ·
[Blooming](patterns/blooming.md)

**Uniformity & Panel Defects**
[Uniformity](patterns/uniformity.md) ·
[Black Uniformity](patterns/black-uniformity.md) ·
[White Uniformity](patterns/white-uniformity.md) ·
[Color Uniformity](patterns/color-uniformity.md) ·
[Backlight Bleeding](patterns/backlight-bleeding.md) ·
[Dead Pixels](patterns/dead-pixels.md) ·
[Viewing Angle](patterns/viewing-angle.md)

**Color & Gamuts**
[sRGB](patterns/srgb.md) ·
[Adobe RGB](patterns/adobe-rgb.md) ·
[DCI-P3](patterns/dci-p3.md) ·
[Rec.709](patterns/rec709.md) ·
[Rec.2020](patterns/rec2020.md) ·
[HDR](patterns/hdr.md) ·
[Chroma Resolution](patterns/chroma-resolution.md) ·
[Cross-Color](patterns/cross-color.md)

**Motion**
[Response Time](patterns/response-time.md) ·
[Ghosting](patterns/ghosting.md) ·
[Motion Resolution](patterns/motion-resolution.md)

---

## Documentation Format

Each pattern page (available or reference) aims to follow the same structure: **Purpose**, **Theory**, **Procedure**, **Expected Result**, **Common Problems**, **Professional Notes**. Not every reference-library page has all sections filled in yet — all 11 available patterns now have a dedicated page; expanding the reference library is tracked as ongoing documentation work.
