# Traditional WinForms Tray Menu

**Status**: done
**Serial**: T1
**Spec**: ../spec.md
**Depends on**: none - this task stands alone

## Goal

Right-clicking the tray icon shows a plain, traditional WinForms context menu instead of the custom flat-light theme, with every item still performing its existing action.

## Acceptance

- [x] `TrayIconService.ShowContextMenu()` builds a plain `ContextMenuStrip`: no `Renderer`, no custom `Font`, no `ImageScalingSize`, no `Tag = TrayMenuItemRole.*`, no custom `Padding`, no bold header font.
- [x] Language items (English/Chinese) keep single-selection radio behavior via native `Checked`; only one is checked at a time.
- [x] Always on Top and Start at login keep `CheckOnClick`/native check behavior; Exit still shuts the app down.
- [x] The two leading informational rows ("TinyTrans" title and shortcut hint) remain as native `Enabled = false` items above the first separator.
- [x] `TinyTrans/TrayMenuRenderer.cs` is deleted (renderer, `TrayMenuColors`, `TrayMenuColorTable`, `TrayMenuItemRole`, `TrayCheckMark`).
- [x] `TinyTrans.Core/TrayMarkGeometry.cs` and `TinyTrans.Core.Tests/TrayMarkGeometryTests.cs` are deleted; the Core test suite stays green.
- [x] The Windows-only solution builds clean (`-p:EnableWindowsTargeting=true`) with no dangling references to the removed types.

## Notes

Traceability: spec stories 1 and 2; spec "Technical decisions" bullets for `TrayIconService.cs`, deleting `TrayMenuRenderer.cs`, and removing `TrayMarkGeometry` + its tests.

- This reverses the `2026-06-12-beautify-tray-menu` work; the deleted code is reconstructable from git history if the styled menu is ever restored.
- Per spec Q1 assumption: keep the two disabled header rows as plain greyed items rather than dropping them.
- Appearance itself is native WinForms owner-draw with no headless contract; final visual confirmation is manual on a real Windows host. Code-level acceptance is verifiable by build + inspection.
