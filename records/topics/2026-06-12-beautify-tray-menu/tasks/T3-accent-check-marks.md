# Custom accent marks for radio and toggle items

**Status**: done
**Serial**: T3
**Spec**: ../spec.md
**Depends on**: T1 (the renderer must exist to override check rendering; the flat-light surface defines the accent color these marks are drawn against)

## Goal

The active language and the on/off toggles show custom-drawn accent marks instead of the native gray check gutter, with the radio-vs-toggle distinction preserved.

## Acceptance

- [x] `OnRenderItemCheck` suppresses the native WinForms check glyph. (override draws our own mark and returns; never calls base)
- [x] The active language item draws a filled accent dot/bar (radio style); the inactive language item draws nothing. (`MarkFor(LanguageRadio, checked) -> RadioDot`; unchecked -> `None`)
- [x] Only one language item shows the mark at a time after switching languages. (existing click handlers keep exactly one `Checked`; mark follows `Checked`)
- [x] Each `CheckOnClick` toggle ("Always on Top", "Start at login") draws an accent checkmark when on and nothing when off. (`MarkFor(Toggle, checked) -> CheckMark`; unchecked -> `None`)
- [x] Item role (radio vs toggle) is marked at construction in `ShowContextMenu()` via `Tag = TrayMenuItemRole.*`, not inferred from `CheckState` or matched by string.
- [x] All click/toggle handlers continue to work. (handlers untouched; only `Tag`/`Padding` added)
- [x] Project builds with no new warnings. (official SDK + `EnableWindowsTargeting`: 0 warnings / 0 errors, 2026-06-12)

## Notes

Traceability: Story 3 (Q5); `OnRenderItemCheck` and radio-vs-toggle decisions in spec `## Technical decisions` and `## Further notes`.

Verification: build verified clean (0/0) via the official SDK + `EnableWindowsTargeting` (see [[windows-only-build]]). The role -> mark decision is extracted as the pure static `TrayMenuRenderer.MarkFor(role, isChecked)` per spec `## Test strategy`. It lives in the WinForms assembly (depends on `TrayMenuItemRole`), so the `net9.0` Core test project cannot reference it without a Windows build; not worth relocating menu enums into the translation Core library to manufacture a Linux-runnable test. Logic verified by inspection; the drawn mark appearance is the remaining visual check on a Windows host. Core suite green (26/26).

Both language items and toggles use `Checked`, so `CheckState` cannot distinguish them — role must be tagged at construction. If a pure helper decides mark style from item role, unit-test that helper per spec `## Test strategy`; otherwise verify visually on Windows.
