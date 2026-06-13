# Tray Marks Still Left-Aligned - Second Investigation

**Date**: 2026-06-13
**Prompted by**: Bug report with screenshot showing checkmarks and dots still left-aligned after the 2026-06-12 fix.

## Loop

Runtime logging from Windows: instrumented `TrayMenuRenderer.OnRenderImageMargin` and `OnRenderItemCheck` to capture actual values during rendering.

**Reproduction**: Consistent across all renders. Log shows:
```
_gutterWidth=25, gutterWidth=25, rect={X=1,Y=7,Width=24,Height=24}, markSize=24, anchor=(0.5, 7, 24)
```

## Root Cause

**Line**: `TinyTrans/TrayMenuRenderer.cs:162`
```csharp
float markSize = Math.Min(gutterWidth, rect.Height);
```

**Issue**: The `markSize` calculation uses `rect.Height` (24px) which is almost the entire gutter width (25px). This leaves only 1px of horizontal space, resulting in:
- `anchor.X = (25 - 24) / 2 = 0.5px` from the left edge
- The mark appears left-aligned because it occupies 96% of the gutter strip

**Why the 2026-06-12 fix didn't work**: The fix correctly switched from drawing in the narrow `ImageRectangle` to drawing in the full gutter strip, and the centering math in `CenteredInGutter` is correct. However, the mark size calculation defeats the fix by making the mark so large that "centered" is indistinguishable from "left-aligned."

**Math verification**:
- Current: `markSize=24, anchor.X=0.5` (essentially left-aligned)
- Proper (50% of gutter): `markSize=12.5, anchor.X=6.25` (visibly centered)
- Proper (60% of gutter): `markSize=15, anchor.X=5.0` (visibly centered)

## Confirmed Hypothesis

**Hypothesis #3 from initial list**: The `markSize` calculation produces a value too close to `gutterWidth`, leaving insufficient space for visible centering.

**Falsified hypotheses**:
1. ❌ `_gutterWidth` not captured correctly - log shows it's 25px (reasonable)
2. ❌ Application not restarted - confirmed running new build with logging
3. ✅ `markSize` too large - **CONFIRMED**

## Fix Required

Replace line 162 with:
```csharp
float markSize = Math.Min(gutterWidth * 0.5f, rect.Height);
```

This ensures the mark occupies ~50% of the gutter width, leaving visible space on both sides. The mark will be centered at `anchor.X ≈ 6.25px` instead of `0.5px`.

Alternative: Use 60% (`* 0.6f`) for a slightly larger mark, still visibly centered.

## Next Action

Use `tdd` to:
1. Add a test that fails when `markSize` is too close to `gutterWidth`
2. Implement the fix
3. Verify visually on Windows that marks now appear centered
