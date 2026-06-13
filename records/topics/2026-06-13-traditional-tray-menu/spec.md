# Traditional Tray Menu and No Main-Window Context Menu

**Status**: done

## Problem

Two recently-added UI behaviors are being walked back:

1. The tray right-click menu (`TrayIconService.ShowContextMenu()`) currently uses a custom flat-light style: a `TrayMenuRenderer` that recolors the surface/hover/separators/border, draws custom accent marks in place of the native check gutter, styles two disabled header rows, and sets Segoe UI with roomier padding. The user now wants the plain, traditional WinForms `ContextMenuStrip` look instead.
2. The main window's output box exposes a right-click context menu (a single "Copy" item in `MainWindow.xaml`). The user wants no right-click menu in the main window at all.

## Solution

Revert the tray menu to the default WinForms appearance: no custom renderer, no custom-drawn marks, no header styling, no custom font/padding overrides. Items keep their existing behavior and native checked state (language items show the native check/radio glyph, toggles show the native check). Separately, remove the right-click context menu from the main window so right-clicking inside it does nothing (no Copy/Cut/Paste popup).

## User stories

1. As a user, I want the tray right-click menu to use the standard, traditional WinForms look, so that it matches default OS menu behavior rather than a custom theme. (Q1)
2. As a user, I want the tray menu's items (language selection, Always on Top, Start at login, Exit) to keep working exactly as before, so that only the appearance changes, not the behavior. (Q1)
3. As a user, I want no right-click context menu to appear in the main window, so that right-clicking does nothing instead of showing a Copy popup. (Q2)

## Technical decisions

- **`TinyTrans/TrayIconService.cs` (`ShowContextMenu`)** — strip the styling, keep the structure:
  - Remove `Renderer = new TrayMenuRenderer()`, the custom `Font`, and `ImageScalingSize` overrides; construct a plain `ContextMenuStrip`.
  - Remove all `Tag = TrayMenuItemRole.*`, custom `Padding` (`ItemPadding`/`HeaderPadding`), and the `FontStyle.Bold` header font.
  - Keep the existing items, click handlers, `Checked`/`CheckOnClick` logic, and `menu.Show(Cursor.Position)`. The two leading informational rows ("TinyTrans" title, shortcut hint) remain as `Enabled = false` items (rendered as native greyed rows) unless dropped per Q1's note.
- **Delete `TinyTrans/TrayMenuRenderer.cs`** — the renderer, `TrayMenuColors`, `TrayMenuColorTable`, `TrayMenuItemRole`, and `TrayCheckMark` become unused once styling is removed.
- **`TinyTrans.Core/TrayMarkGeometry.cs` and `TinyTrans.Core.Tests/TrayMarkGeometryTests.cs`** — now-dead code whose only consumer was `TrayMenuRenderer`; remove both so the Core project and test suite carry no orphaned geometry. (See Q1 note if retention is preferred.)
- **`TinyTrans/MainWindow.xaml`** — remove the `OutputTextBox.ContextMenu` block (the `ContextMenu` + "Copy" `MenuItem`). To suppress the default WPF text-box context menu as well (so right-click shows nothing rather than the built-in Cut/Copy/Paste), set `ContextMenu="{x:Null}"` on `OutputTextBox` (and on `InputTextBox` for consistency).
- **`TinyTrans/Styles.xaml`** — `FlatContextMenuStyle` and `FlatMenuItemStyle` lose their only consumer; remove them. `IconButtonStyle`, colors, and brushes are unaffected.

## Test strategy

- Tray menu appearance is native WinForms owner-draw with no headless-testable contract; verify manually on Windows that the menu renders with the default system style and that every item still performs its action (language switch with single-selection, Always on Top toggle, Start at login toggle, Exit).
- The Core test suite must stay green after removing `TrayMarkGeometryTests`; run it to confirm no other references break.
- Build the Windows-only project (`-p:EnableWindowsTargeting=true`) to confirm no dangling references to the deleted renderer/geometry types remain.
- Manual check: right-clicking the main window's input and output boxes shows no context menu.

## Out of scope

- Tray icon, left-click toggle behavior, hotkey registration, and translation flow — unchanged.
- The palette colors/brushes and `IconButtonStyle` in `Styles.xaml` — only the two flat-menu styles are removed.
- Any new tray menu features or reordering of items.

## Further notes

- This spec reverses `records/topics/2026-06-12-beautify-tray-menu/spec.md`; that spec is now `dropped` (the custom renderer it introduced has been removed).
- (Q1) The two disabled header rows are kept as plain greyed items for the traditional look. If the user prefers a cleaner native menu, they could be dropped along with the leading separator — flagged as a minor, easily-reversible choice, not blocking. Assumption: keep them.
- Removing `TrayMarkGeometry` from Core assumes no future reuse; it is reconstructable from git history if the styled menu is ever restored.
- Interpretation of "remove the right-click menu of the main window": suppress all right-click context menus in the window (set `ContextMenu="{x:Null}"`), not merely swap the custom Copy menu back to the WPF default. Surfaced here rather than asked, as it is low-risk and reversible.
