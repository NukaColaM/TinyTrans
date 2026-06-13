# Distinct header for title and shortcut hint

**Status**: done
**Serial**: T4
**Spec**: ../spec.md
**Depends on**: T1 (the renderer's text-rendering override and font setup must exist before the header rows can be given distinct text styling)

## Goal

The "TinyTrans" title and the shortcut-hint row read as a distinct header block rather than greyed-out disabled rows.

## Acceptance

- [x] The two leading disabled informational rows are marked at construction via `Tag = TrayMenuItemRole.HeaderTitle` / `HeaderHint`, so the renderer special-cases them without string matching.
- [x] `OnRenderItemText` draws the "TinyTrans" title in a subtle bold/subdued header style. (recolored to `#202020`; bold font set on the item at construction)
- [x] The shortcut-hint row renders in muted `#888888`. (`HeaderHint` -> `TrayMenuColors.Subdued`)
- [x] The header block is visually set apart from the actionable items below. (`HeaderPadding` + bold title + muted hint + following separator)
- [x] Both the registered-shortcut and the "unavailable" hint variants render with the header-hint style. (both label variants get `Tag = HeaderHint`)
- [x] Project builds with no new warnings. (official SDK + `EnableWindowsTargeting`: 0 warnings / 0 errors, 2026-06-12)

## Notes

Traceability: Story 4 (Q6); header-rows and `OnRenderItemText` decisions in spec `## Technical decisions`.

Verification: build verified clean (0/0) via the official SDK + `EnableWindowsTargeting` (see [[windows-only-build]]). Code criteria met by inspection of `TinyTrans/TrayMenuRenderer.cs` (`OnRenderItemText` header cases) and `TinyTrans/TrayIconService.cs` (header item tags + bold font + `HeaderPadding`). The drawn header appearance is the remaining visual check on a Windows host. Core suite green (26/26).

Reuses the construction-time item tagging approach from T3. Verification is manual/visual on Windows, including the shortcut-unavailable variant from `ShowContextMenu()`.
