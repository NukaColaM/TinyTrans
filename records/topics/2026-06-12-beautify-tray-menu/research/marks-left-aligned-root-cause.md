# Tray Marks Still Left-Aligned Research

**Date**: 2026-06-12
**Prompted by**: Bug report "Checkmarks and dots are still left-aligned" after the centering change in `TrayMarkGeometry` + `OnRenderItemCheck`.

## What was investigated

Why the tray menu's radio dot and checkmark still appear left-aligned even though
`TrayMarkGeometry.RadioDot`/`CheckMark` are unit-tested to center within their bounds.
GUI rendering is not reproducible on this Linux host, so the loop is a deterministic
static analysis of the draw path and a before/after comparison of the placement math.

## Findings

- `TrayMenuRenderer.OnRenderItemCheck` draws into `e.ImageRectangle`
  (`TinyTrans/TrayMenuRenderer.cs`). In WinForms, `ImageRectangle` is the
  **check/image margin gutter**: a narrow (~16-22px) column pinned to the **left
  edge** of each menu row. Nothing in `ShowContextMenu()` widens or repositions it.
- The geometry helpers center the mark *within the rectangle they are given*. The
  renderer gives them the left gutter, so the mark is centered **inside the left
  gutter** - which still reads as left-aligned relative to the full row.
- Decisive point - the change was a horizontal no-op:
  - OLD dot (inline): `x = rect.X + (rect.Width - diameter) / 2`
  - NEW dot (`RadioDot`): `dotX = x + (width - diameter) / 2f`
  - The horizontal formula is byte-identical (only int -> float changed). The
    checkmark's horizontal center moved from ~0.51 to ~0.50 of the gutter
    (negligible). So the marks did not move horizontally; hence "still" left-aligned.
- The passing unit tests verify centering within an arbitrary passed rectangle
  (`(0,0,16,16)`, `(10,4,20,12)`). They are correct about the helper but assert the
  wrong coordinate space: the real anchor at runtime is the left gutter, not the row.

## Implications

- Root cause is the **coordinate space**, not the helper math: marks are centered in
  the left check/image gutter, which is pinned left, so they cannot appear centered
  relative to the row.
- A fix must change behavior (out of scope for diagnose): either
  (a) draw the mark relative to the item's full content width rather than
  `e.ImageRectangle`, or
  (b) widen / reposition the check margin so its center is where the marks should sit.
- The `tdd` slice should add a behavior test that pins placement against the
  **item/content width** (the row), not an abstract rectangle, so a green test
  actually corresponds to a centered-on-screen mark. Note the chosen target: a
  conventional menu keeps checks in the left gutter, so confirm with the user whether
  "central" means centered in a (possibly widened) gutter or centered across the row.

## Resolution (2026-06-12)

Fixed via `tdd`. The user clarified the target: center the mark **in the image-margin
gutter strip** (menu edge -> content), leaving text left-aligned - the conventional
menu layout, not a full-row group shift.

- New pure helper `TrayMarkGeometry.CenteredInGutter(gutterWidth, gutterHeight, markSize)`
  returns an `AnchorBox` centered in the strip; `RadioDot`/`CheckMark` draw into it.
- `TrayMenuRenderer.OnRenderImageMargin` now captures `e.AffectedBounds.Width` into a
  `_gutterWidth` field; `OnRenderItemCheck` centers the mark in
  `[0, _gutterWidth] x [0, item.Height]` (item-relative) instead of the narrow
  `e.ImageRectangle`. Width is translation-invariant, so the menu-space strip width is
  valid in item-relative space; both callbacks fire in the same paint.
- New tests pin the anchor center to the **strip center**, not an abstract rectangle, so
  green corresponds to on-screen placement. Build 0/0, 33/33 tests pass.
- Verification gap unchanged: the math is tested on Linux, but the rendered pixel result
  still needs a Windows host to confirm.
