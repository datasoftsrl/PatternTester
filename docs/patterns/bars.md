# Bars

## Purpose

Draws a configurable number of bars across the screen, each one a progressively dimmer version of a chosen base color — a simple brightness step wedge in a single hue, oriented in any of four directions. It gives a quick, at-a-glance read on how a display separates adjacent brightness steps within one color channel combination, without the complexity of a full grayscale ramp.

---

## Parameters

| Parameter | Description |
|---|---|
| **Color** | The base color; each bar is a scaled-down (dimmer) version of it, from full brightness down to nearly zero. |
| **Direction** | Which edge the brightest bar starts from: left, right, top or bottom. |
| **# Bars** | How many bars to draw (from 1 up to a maximum of 32). |

---

## Theory

Each bar's brightness is a fraction of the base color, evenly spaced between full intensity and near-zero across the chosen number of bars. This is conceptually the same idea as a grayscale step wedge (see [Grayscale](grayscale.md)), but using an arbitrary base color instead of always white — useful for checking step separation within a single channel (e.g. all-red bars to check red channel banding) or simply as a fast, low-clutter brightness/contrast sanity check that's quicker to read than a full gradient.

---

## Procedure

1. Select **Bars** from the **Pattern** menu.
2. Choose a base color — white or gray is the general-purpose choice; a primary color (red/green/blue) isolates that channel.
3. Set the number of bars — more bars give finer steps but make each individual step harder to distinguish; start around 8–10.
4. Pick a direction that suits how you want to scan the panel (e.g. left-to-right to check for any horizontal brightness drift alongside the intended step pattern).
5. Show fullscreen and verify that every bar is visibly distinct from its neighbors, in order, with no two adjacent bars appearing identical or reversed.

---

## Expected Result

Bars evenly and monotonically stepping down in brightness from one edge to the other, each one clearly distinguishable from its neighbors, with no banding artifacts *within* a bar (which should itself be perfectly flat — see [Single Color](single-color.md) for that specific check) and no unexpected color shift between bars beyond the intended brightness change.

---

## Common Problems

### Crushed steps
Two or more adjacent bars near the dark end appear identical — a sign of black-level crush / lost shadow detail (see [Black Level](black-level.md)).

### Clipped steps
Two or more adjacent bars near the bright end appear identical — highlight clipping (see [White Level](white-level.md)).

### Uneven step spacing
Steps that should be evenly spaced in perceived brightness look bunched at one end and stretched at the other — often a gamma-curve issue (see [Gamma](../patterns/gamma.md)).

---

## Professional Notes

Because Bars uses a single flat color per bar rather than a continuous gradient, it's a faster way to eyeball gross step-separation problems than [Gradient to Black / Two-Color Gradient](gradients.md), at the cost of not showing the finer banding that a smooth gradient reveals. Use Bars for a quick check, and switch to the gradient patterns (or [Gamma](gamma.md)) when you need to characterize the display's tone curve more precisely.
