# Phase

## Purpose

Draws the finest possible alternating black/white line pattern the display can receive — a single line, then a single-line gap, repeated across the whole screen (1 pixel on, 1 pixel off). This is the classic "phase" test pattern historically used to fine-tune the sampling phase of analog VGA connections, and today it doubles as a strict pixel-level test for scaling, sharpness and sub-pixel rendering fidelity on digital displays.

---

## Parameters

| Parameter | Description |
|---|---|
| **Direction** | Orientation of the lines: from the left/right edge draws **vertical** lines; from the top/bottom edge draws **horizontal** lines. |

---

## Theory

On analog (VGA) connections, the display's ADC has to sample the incoming signal at exactly the right moment to reconstruct each pixel correctly; a wrong sampling *phase* causes fine detail to shimmer, blur or show interference patterns, while a coarser image can still look fine. A 1-pixel-on/1-pixel-off pattern is the highest-frequency detail a display can be asked to reproduce, which makes it the most sensitive possible test for phase error — hence the pattern's name, and its long history as a standard CRT/VGA calibration tool.

On modern digital connections (HDMI/DisplayPort) there is no analog sampling phase to adjust, but the pattern is still useful: it stresses the scaler at its sharpest possible input, which is exactly the condition that reveals scaling artifacts, moiré, and how aggressively (or not) a display's sharpness/edge-enhancement processing is altering fine detail.

---

## Procedure

1. Select **Phase** from the **Pattern** menu.
2. Choose a direction — vertical lines (left/right) are the more commonly used orientation, but check both if you have the time, since scaling/sampling behavior is not always symmetric between the two axes.
3. Show fullscreen at the display's **native resolution** — this pattern is only meaningful pixel-for-pixel; any scaling (by the GPU or the display) defeats the test.
4. Look for the lines to be crisp, evenly spaced and of consistent width and brightness across the whole screen, with no shimmering, moiré, or areas where adjacent lines appear to merge.

---

## Expected Result

A perfectly regular, static grid of 1-pixel lines, uniform in width and spacing everywhere on screen, with no flickering, shimmering, color fringing, or areas of merged/blurred lines.

---

## Common Problems

### Shimmering or flicker
On an analog connection, this is the textbook sign of incorrect ADC sampling phase, correctable via the display's own phase/clock adjustment if it has one. On a digital connection it usually points to aggressive or mismatched scaling rather than phase in the strict analog sense.

### Merged or missing lines
Some lines blend into their neighbors instead of staying distinct — a sign the display (or an intermediate scaler) is not rendering true native pixel resolution, or that overly strong noise reduction/sharpening is smoothing the fine detail away.

### Moiré or color fringing
Interference patterns or unexpected color appearing at the line edges — see [Moiré](moire.md) for more on this specific artifact.

---

## Professional Notes

Always verify the source resolution matches the display's native resolution exactly before judging this pattern — any mismatch introduces scaling, and scaling artifacts will be misread as a display fault. This is also one of the most useful patterns for spotting whether a video path (cable, switch, capture device, scaler) silently resamples the image somewhere along the chain, since only a fully pixel-accurate path reproduces it cleanly.
