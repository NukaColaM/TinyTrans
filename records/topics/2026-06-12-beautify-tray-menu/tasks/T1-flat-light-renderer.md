# Flat light renderer with palette colors

**Status**: done
**Serial**: T1
**Spec**: ../spec.md
**Depends on**: none - this task stands alone

## Goal

The tray right-click menu renders with the app's flat light palette (surface, hover/selection, separators, border) instead of the default gray WinForms chrome.

## Acceptance

- [x] A new `TrayMenuRenderer : ToolStripProfessionalRenderer` (with a custom `ProfessionalColorTable`) exists in the `TinyTrans` namespace.
- [x] `ShowContextMenu()` assigns `menu.Renderer = new TrayMenuRenderer(...)`.
- [x] Menu surface background renders `#F5F5F5`. (`TrayMenuColorTable.ToolStripDropDownBackground` + `OnRenderMenuItemBackground` surface fill)
- [x] Hover/selection fill renders flat `#E0E0E0` (no gradient). (selection gradient begin/middle/end all set to `#E0E0E0`; flat fill in `OnRenderMenuItemBackground`)
- [x] Separators and the 1px menu border render `#CCCCCC`. (`SeparatorDark`/`MenuBorder` + `OnRenderToolStripBorder`)
- [x] All existing menu items still appear in the same order and every click handler (language switch, always-on-top, start-at-login, exit) still works. (item construction and Click handlers unchanged; only `Renderer`, `Font`, `Tag`, `Padding` added)
- [x] Project builds (`dotnet build`) with no new warnings introduced by the renderer. (verified: official SDK + `EnableWindowsTargeting`, full solution 0 warnings / 0 errors)

## Notes

Traceability: Story 1 (Q1, Q2, Q4); palette source and renderer-overrides decisions in spec `## Technical decisions`.

Verification: code criteria met by inspection of `TinyTrans/TrayMenuRenderer.cs` and `TinyTrans/TrayIconService.cs`. Build verified — full solution compiles with 0 warnings / 0 errors via the official SDK + `EnableWindowsTargeting` (see [[windows-only-build]]). Core test suite green (26/26). Only the visual/UI appearance remains to be confirmed on a real Windows host.

This is the foundation task: it stands up the renderer type, wires it into `ShowContextMenu()`, and proves the flat-light recolor end to end. Later tasks extend the same renderer. Color overrides involved: `ProfessionalColorTable` for menu/image-margin/separator colors, `OnRenderMenuItemBackground` for flat selection fill, `OnRenderToolStripBorder` for the border. Hard-code `System.Drawing.Color` equivalents of the `Styles.xaml` hex values (duplication is accepted per spec). Verification is manual/visual on Windows.
