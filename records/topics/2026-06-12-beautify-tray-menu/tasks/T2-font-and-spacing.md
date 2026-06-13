# Segoe UI font and roomier spacing

**Status**: done
**Serial**: T2
**Spec**: ../spec.md
**Depends on**: T1 (the renderer and its menu wiring must exist before typography and item spacing are applied on top of the flat-light surface)

## Goal

The tray menu uses Segoe UI at ~10pt with roomier item rows and rounded selection highlights, so it reads as modern rather than dated.

## Acceptance

- [x] `ContextMenuStrip.Font` is set to Segoe UI ~10pt in `ShowContextMenu()`.
- [x] Actionable item rows have visibly increased padding/height versus the WinForms default. (`ItemPadding` = 4px top/bottom on every actionable item)
- [x] The hover/selection highlight is drawn as a rounded rectangle (not a square fill). (`OnRenderMenuItemBackground` -> `RoundedRectangle`, 4px radius, anti-aliased)
- [ ] Text remains vertically centered and not clipped at the new row height. -> visual check, BLOCKED on Windows.
- [x] All click handlers continue to work and item order is unchanged. (construction/handlers untouched)
- [x] Project builds with no new warnings. (official SDK + `EnableWindowsTargeting`: 0 warnings / 0 errors, 2026-06-12)

## Notes

Traceability: Story 2 (Q3, Q4); font/spacing and rounded-selection decisions in spec `## Technical decisions`.

Verification: build verified clean (0/0) via the official SDK at `/home/dev/.dotnet-official` with `-p:EnableWindowsTargeting=true` (see [[windows-only-build]]). Code criteria met by inspection of `TinyTrans/TrayMenuRenderer.cs` (rounded selection) and `TinyTrans/TrayIconService.cs` (font + `ItemPadding`). Vertical-centering at the new row height is the one remaining visual check, confirmable only on a Windows host. Core test suite green (26/26).

Builds on T1's `OnRenderMenuItemBackground` to round the highlight corners. Roomier rows via `ToolStripMenuItem.Padding` and/or the menu font. Keep Segoe UI as the family (native Windows UI font); spec allows Segoe UI Variable if available. Verification is manual/visual on Windows.
