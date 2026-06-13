# Beautify Tray Icon Menu

**Status**: dropped

> Superseded by `../2026-06-13-traditional-tray-menu/spec.md` (2026-06-13): the custom flat-light tray renderer and its supporting geometry were removed in favor of the traditional WinForms menu. The four tasks below were implemented but have since been reverted.

## Problem

The tray icon right-click menu (`TrayIconService.cs` `ShowContextMenu()`) renders with the default WinForms `ContextMenuStrip` look: flat gray system chrome, a dated check/radio gutter, tight default spacing, and the system menu font. It clashes with the app's clean light WPF main window and reads as unstyled rather than intentional.

## Solution

Apply a modern flat light style to the tray menu so it visually matches the main window. Introduce a custom `ToolStripRenderer` that recolors the surface, hover/selection, separators, and border using the app's existing light palette; uses Segoe UI with roomier item spacing and rounded selection highlights; replaces the native check gutter with custom-drawn accent marks (a filled accent mark for the active language radio item and an accent checkmark for the on/off toggles); and presents the two disabled informational rows as a distinct header (subtle title plus muted shortcut hint).

## User stories

1. As a user, I want the tray right-click menu to use a clean flat light style matching the main window, so that the app looks cohesive and intentional. (Q1, Q2, Q4)
2. As a user, I want roomier item spacing, a Segoe UI font, and rounded hover highlights in the tray menu, so that it feels modern rather than dated. (Q3, Q4)
3. As a user, I want the active language and on/off toggles shown with custom accent marks instead of the native gray check gutter, so that the menu state reads clearly in the new style. (Q5)
4. As a user, I want the menu title and shortcut hint presented as a distinct header, so that the menu reads as designed rather than as greyed-out rows. (Q6)

## Technical decisions

- **New type `TrayMenuRenderer : ToolStripProfessionalRenderer`** in `TinyTrans` (WinForms). Subclassing `ToolStripProfessionalRenderer` (with a custom `ProfessionalColorTable`) gives control over background, hover/selection fill, separators, image margin, and borders while keeping correct layout/measurement behavior.
- **Palette source.** The renderer hard-codes `System.Drawing.Color` equivalents of the WPF `Styles.xaml` resources, since WinForms cannot read the WPF resource dictionary:
  - Surface background: `#F5F5F5` (`WindowBackgroundColor`)
  - Hover/selection fill: `#E0E0E0` (`ButtonHoverColor`)
  - Pressed/active accent: `#D0D0D0` (`ButtonPressedColor`) — base for accent marks
  - Border/separator: `#CCCCCC` (`WindowBorderColor`)
  - Subdued/header hint text: `#888888` (`SubduedTextColor`)
- **Renderer overrides:**
  - `ProfessionalColorTable` subclass for menu background, selection gradient (flat fill), separator, and image-margin colors.
  - `OnRenderMenuItemBackground` — flat rounded-rectangle hover/selection highlight (no gradient).
  - `OnRenderItemCheck` — suppress the native check glyph; custom-draw a filled accent dot/bar for radio-style language items and an accent checkmark for `CheckOnClick` toggles. The renderer distinguishes the two by item identity rather than by `CheckState` (both are `Checked`).
  - `OnRenderToolStripBorder` — `#CCCCCC` 1px border.
  - `OnRenderItemText` — header/hint text colors for the disabled informational rows.
- **Font and spacing.** Set `ContextMenuStrip.Font` to Segoe UI ~10pt and increase item padding/height (via `ToolStripMenuItem.Padding` and/or measured layout) for roomier rows.
- **Header rows.** The first two disabled items ("TinyTrans" title, shortcut hint) are marked so the renderer treats them as a header block: title in a subtle bold/subdued style, hint in `#888888`, set apart from actionable items. Item identity is established at construction in `ShowContextMenu()` (e.g. via `Tag` or dedicated fields) so the renderer can special-case them without string matching.
- **Wiring.** `ShowContextMenu()` assigns `menu.Renderer = new TrayMenuRenderer(...)` and the new font; existing item construction, click handlers, checked-state logic, and `menu.Show(...)` behavior are preserved unchanged.

## Test strategy

- This is WinForms owner-draw rendering with no headless-testable public contract; verification is primarily manual/visual on Windows.
- Manual checks: surface/hover/separator/border colors match the palette; Segoe UI applied; rows are roomier; active language shows the accent radio mark and only one language is marked at a time; both toggles show the accent checkmark when on and nothing when off; header title and hint are visually distinct; clicking each item still triggers its existing behavior (language switch, always-on-top, start-at-login, exit).
- If any logic is extracted from `ShowContextMenu()` (e.g. a pure helper deciding mark style from item role), unit-test that helper; otherwise no automated UI test is added.

## Out of scope

- The output textbox WPF "Copy" `ContextMenu` (`MainWindow.xaml`) — not touched.
- The popup window's own outer rounded corners, drop shadow, and open/close animation — OS-controlled, not reachable via `ToolStripRenderer`.
- OS light/dark theme detection — fixed light palette only.
- Centralizing the palette across WPF and WinForms — the renderer duplicates the hex values by design; deduplication is potential follow-on work.

## Further notes

- The radio-vs-toggle distinction (Q5) cannot be inferred from `CheckState` alone since both language items and toggle items use `Checked`; item role must be marked at construction for the renderer to draw the correct mark.
- Palette duplication between `Styles.xaml` and `TrayMenuRenderer` is an accepted minor contradiction with the single-source-of-truth ideal; noted as follow-on cleanup, not blocking.
- Implementation status (2026-06-12): all four tasks (T1-T4) implemented in `TinyTrans/TrayMenuRenderer.cs` (new) and `TinyTrans/TrayIconService.cs` (wiring, role tags, font, padding). Code-level acceptance verified by inspection.
- Build verified clean (0 warnings / 0 errors) by compiling the Windows-only project on the Linux dev host with an official .NET SDK at `/home/dev/.dotnet-official` plus `-p:EnableWindowsTargeting=true` (see memory `windows-only-build`). This caught and fixed a real compile error inspection missed (`System.Drawing.FontStyle` vs `System.Windows.FontStyle` ambiguity in `TrayIconService.cs`). Core test suite green (26/26).
- Remaining acceptance gap: the actual on-screen appearance (vertical centering at the new row height; the drawn radio dot / checkmark glyphs; that colors and the header block read as intended) can only be confirmed by running the GUI on a real Windows host — cross-compiling on Linux verifies it compiles, not how it looks. Spec stays `active` until that visual pass is done.
