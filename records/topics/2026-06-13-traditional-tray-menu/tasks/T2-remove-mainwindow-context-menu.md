# Remove Main-Window Right-Click Menu

**Status**: done
**Serial**: T2
**Spec**: ../spec.md
**Depends on**: none - independent of T1; touches only the WPF main window and styles

## Goal

Right-clicking anywhere in the main window shows no context menu at all (no custom Copy popup and no built-in Cut/Copy/Paste).

## Acceptance

- [x] The `OutputTextBox.ContextMenu` block (the `ContextMenu` + "Copy" `MenuItem`) is removed from `MainWindow.xaml`.
- [x] `OutputTextBox` and `InputTextBox` set `ContextMenu="{x:Null}"` so no context menu (custom or WPF default) appears on right-click.
- [x] `FlatContextMenuStyle` and `FlatMenuItemStyle` are removed from `Styles.xaml`; `IconButtonStyle`, colors, and brushes are untouched.
- [x] The Windows-only solution builds clean with no unresolved `StaticResource` references to the removed styles.

## Notes

Traceability: spec story 3; spec "Technical decisions" bullets for `MainWindow.xaml` and `Styles.xaml`.

- Per spec interpretation: "remove the right-click menu" means suppress all right-click menus in the window (`{x:Null}`), not swap the custom menu back to the WPF default.
- Manual confirmation on Windows: right-clicking both the input and output boxes produces no popup; text selection and the Copy button still work.
