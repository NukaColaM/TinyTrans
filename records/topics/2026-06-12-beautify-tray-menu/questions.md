# Beautify Tray Icon Menu Questions

**Date**: 2026-06-12

## Questions
| # | Question | Answer |
|---|---|---|
| Q1 | Which right-click menu should be beautified? | The tray icon menu only (`TrayIconService.cs` `ShowContextMenu()`), a WinForms `ContextMenuStrip`. Not the output textbox WPF menu. |
| Q2 | What visual direction? | Modern flat light, matching the app's existing light palette (`#F5F5F5` surface, `#E0E0E0` hover) from `Styles.xaml`. No OS theme detection. |
| Q3 | How deep should the customization go? | Color + typography + spacing: custom renderer overriding colors plus a nicer font, increased item padding/height, and rounded selection highlights. No full owner-draw custom popup. |
| Q4 | Font and accent source? | Sensible defaults: Segoe UI Variable / Segoe UI at ~10pt; surfaces use the existing light palette; hover/selection accent derived from `ButtonHoverColor` (`#E0E0E0`) / `ButtonPressedColor` (`#D0D0D0`). |
| Q5 | How should checked items look (language radio + on/off toggles)? | Custom-drawn accent marks: filled accent dot/bar for the active language, accent checkmark for the on/off toggles. Replace the native gray check gutter. Preserve the radio-vs-toggle visual distinction. |
| Q6 | How should the disabled informational rows (title + shortcut hint) look? | Style as a distinct header: "TinyTrans" as a subtle bold/subdued title, the shortcut hint in muted `SubduedTextColor` (`#888`), set apart from actionable items. |

## Stories
1. As a user, I want the tray right-click menu to use a clean flat light style matching the main window, so that the app looks cohesive and intentional. (Q1, Q2, Q4)
2. As a user, I want roomier item spacing, a Segoe UI font, and rounded hover highlights in the tray menu, so that it feels modern rather than dated. (Q3, Q4)
3. As a user, I want the active language and on/off toggles shown with custom accent marks instead of the native gray check gutter, so that the menu state reads clearly in the new style. (Q5)
4. As a user, I want the menu title and shortcut hint presented as a distinct header, so that the menu reads as designed rather than as greyed-out rows. (Q6)

## Caveats
- Target is a WinForms `ContextMenuStrip`. The popup window's own outer rounded corners, drop shadow, and open/close animation are OS-controlled and out of scope; styling is limited to WinForms `ToolStripRenderer` hooks (background, hover/selection, separators, check/image margin, borders, fonts, padding).
- Palette values currently live in WPF `Styles.xaml` resources; the WinForms renderer will hard-code equivalent `Color` values since it cannot read the WPF resource dictionary directly.
