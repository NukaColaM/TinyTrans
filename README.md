# TinyTrans

A lightweight WPF desktop translation tool that lives in your system tray.  
Type or paste text, hit `Ctrl+Enter`, and get an instant LLM-powered translation — automatically copied to your clipboard.

## Features

- Borderless floating window with Windows 11 rounded-corner support, no taskbar entry, and no Alt+Tab entry
- System tray resident — left-click toggles the window, right-click for menus
- Multiline input; **Ctrl+Enter** translates, plain Enter adds line breaks
- Global **Ctrl+Alt+T** shortcut toggles the window
- Translation auto-copied to clipboard
- Auto-swap: if source language matches your target, it inverts (EN ↔ ZH)
- Always-on-top toggle, start-at-login toggle
- Remembers window position across monitors; single-instance aware

## Quick Start

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0), Windows (WSL can build/test with `-p:EnableWindowsTargeting=true` but cannot run the app).

```bash
# Build & run
dotnet build TinyTrans.sln
dotnet run --project TinyTrans

# Test
dotnet test TinyTrans.sln

# Publish (framework-dependent single-file exe)
dotnet publish TinyTrans -c Release -r win-x64
```

For WSL/Linux, add `-p:EnableWindowsTargeting=true` to every `dotnet` command.

Add `--self-contained true` to `dotnet publish` to bundle the .NET runtime.

## Configuration

On first run a `config.json` is created next to the executable:

```json
{
  "Endpoint": "https://api.deepseek.com/v1/chat/completions",
  "Model": "deepseek-v4-flash",
  "ApiKey": "sk-your-key-here",
  "AlwaysOnTop": false,
  "LastTargetLanguage": "EN"
}
```

Any OpenAI-compatible API works (DeepSeek, OpenAI, Ollama, etc.).

## Usage

1. Launch — the window appears near the bottom-right of the screen.
2. Type or paste text; press **Ctrl+Enter** to translate.
3. The result appears and is already on your clipboard.
4. Press **Ctrl+Alt+T** or left-click the tray icon to show/hide the window.
5. Right-click the tray icon for language, always-on-top, start-at-login, or exit.

## Architecture

```
TinyTrans.sln
├── TinyTrans.Core          Domain logic (providers, config, orchestrator)
├── TinyTrans                WPF desktop app
└── TinyTrans.Core.Tests     xUnit tests
```

| Module | Responsibility |
|--------|---------------|
| `OpenAiCompatibleTranslationProvider` | Calls the LLM chat API |
| `TranslationOrchestrator` | Target-language state, auto-swap, translation flow |
| `AppConfig` / `AppConfigStore` | Settings values and `config.json` persistence |
| `MainWindow` | Borderless window, input/output, spinner |
| `TrayIconService` | System tray icon and context menu |
| `SingleInstanceCoordinator` | Single-instance enforcement |

## License

MIT
