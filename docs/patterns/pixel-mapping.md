# Pixel Mapping

## Purpose

Pixel Mapping verifies that one image pixel corresponds exactly to one physical display pixel.

This test is essential for modern LCD and OLED displays.

---

# Typical Uses

- Native resolution verification
- Scaling detection
- GPU configuration
- Monitor setup

---

# Expected Result

A correct display should show:

- perfectly sharp pixels
- no interpolation
- no blurred edges
- no scaling artifacts

---

# Common Problems

## Interpolation

The image appears soft.

Usually caused by:

- non-native resolution
- monitor scaling
- GPU scaling

---

## Overscan

Outer pixels disappear.

Typical on televisions.

---

## Incorrect Aspect Ratio

Pixels become rectangular instead of square.

---

# Professional Notes

Always test Pixel Mapping using the monitor native resolution.

