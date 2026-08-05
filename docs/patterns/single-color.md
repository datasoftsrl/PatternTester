# Single Color

## Purpose

Fills the entire screen (or, in a multi-column/row layout, every cell) with a single flat color. It is the simplest possible pattern, and precisely because of that, one of the most diagnostic: with no gradient, no lines and no geometry to distract the eye, any deviation from a perfectly flat, uniform field stands out immediately.

---

## Parameters

| Parameter | Description |
|---|---|
| **Color** | The flat color to display. Available presets: black, white, red, green, blue, magenta, yellow, cyan. |

---

## Theory

A display should reproduce a flat color field as, well, flat — the same luminance and chromaticity across the entire panel. In practice, backlight design (edge-lit vs. full-array), panel manufacturing tolerances, and video processing can all introduce visible non-uniformity that a full-color image would normally mask.

- **Black** is the most sensitive test for backlight bleed and IPS glow, since even small amounts of stray light are highly visible against a true black field.
- **White** reveals uneven backlight brightness (clouding) and color-uniformity issues (a tint shifting from one edge to another).
- **Red / Green / Blue** in isolation reveal single-channel uniformity problems and are also the standard way to check for stuck or dead sub-pixels, which are invisible in a busy image but obvious as a single wrong-colored dot on a flat field.
- **Cyan / Magenta / Yellow** (secondary colors) can reveal channel-mixing or convergence issues that a primary color alone would not show.

---

## Procedure

1. Set the pattern to **Single Color** from the **Pattern** menu.
2. Pick a color (start with black and white, then check the primaries individually if you suspect a sub-pixel or uniformity issue).
3. Show it fullscreen (**View → Show Pattern**).
4. Sit at a normal viewing distance and scan the whole screen, corner to corner — dead/stuck pixels and backlight bleed are both easiest to spot at the edges and corners.
5. For backlight bleed specifically, dim the room lights: bleed is far more visible in a dark environment than under normal office lighting.

---

## Expected Result

A perfectly uniform field, same brightness and color from edge to edge, with no visible dots, patches, or gradient across the panel.

---

## Common Problems

### Dead or stuck pixels
A single pixel (or sub-pixel) that stays black (dead) or shows a fixed color regardless of the field being displayed (stuck). Most visible against a contrasting flat field — check each primary color in turn.

### Backlight bleed / clouding
Patches of lighter gray or visible light leakage, usually along the edges or corners, most visible on a black field in a dark room. Common on LCD panels, especially edge-lit ones.

### Color non-uniformity
A visible tint shift across the panel on a white field (e.g. slightly warmer on one side, cooler on the other) — a sign of backlight or panel color-uniformity variance.

---

## Professional Notes

Let the display warm up for a few minutes before judging uniformity — brightness and color can drift noticeably during the first minutes after power-on, especially on LED-backlit panels. For a more rigorous uniformity check across a grid of screen zones rather than a single full-screen field, see [Uniformity](uniformity.md), [Black Uniformity](black-uniformity.md) and [White Uniformity](white-uniformity.md) in the reference library.
