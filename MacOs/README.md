# WinXound — macOS Build Guide

WinXound is a Cocoa-based front-end for [CSound](https://csound.com), built as a native macOS Objective-C/C++ application using a bundled version of [Scintilla](https://www.scintilla.org) for its code editor.

---

## Requirements

### Xcode

- **Xcode 13 or later** is recommended (download from the Mac App Store).
- The project file is in Xcode 3.1 format. When you first open it, Xcode will offer to migrate — accept the migration.
- Command Line Tools must be installed:

  ```sh
  xcode-select --install
  ```

### CSound (required at runtime, not compile time)

WinXound does **not** link against CSound at compile time. It launches `csound` as a subprocess. You must install CSound separately before the app is useful.

1. Download the macOS installer from [https://csound.com/download.html](https://csound.com/download.html).
2. Run the installer — it places:
   - `csound` binary at `/usr/local/bin/csound`
   - `CsoundLib.Framework` at `/Library/Frameworks/CsoundLib.Framework/`

### Optional tools

These are launched by WinXound as external processes. They are not needed to **build**, but are needed for their respective features:

| Tool | Purpose | Default path |
|---|---|---|
| Python 3 | Python script execution | `/usr/bin/python3` |
| Lua | Lua script execution | *(configure in Preferences)* |
| [Cabbage](https://cabbageaudio.com) | CSound plugin development | `/Applications/Cabbage.app` |

---

## Known Issues on Modern macOS

The original project targets macOS 10.7 (Lion) with a 32-bit architecture. Several changes are required to build and run on macOS 10.15 Catalina and later:

| Issue | Required fix |
|---|---|
| `ARCHS = $(ARCHS_STANDARD_32_BIT)` — 32-bit only | In Build Settings, set **Architectures** to `$(ARCHS_STANDARD_64_BIT)` |
| `SDKROOT = macosx10.7` — old SDK | Set **Base SDK** to `macOS (latest)` |
| No `MACOSX_DEPLOYMENT_TARGET` set | Set to `12.0` or your minimum target |
| Manual Reference Counting (non-ARC) | Enable ARC in Build Settings or migrate source manually |
| `WebView` (removed in macOS 14 Sonoma) | Replace usage with `WKWebView` in `wxMainController` |
| Python path points to `/usr/bin/python` (Python 2, removed in macOS 12.3+) | Update default path to `/usr/bin/python3` in Preferences |
| `GCC_MODEL_TUNING = G5` | Remove from Build Settings (PowerPC tuning flag, ignored but generates warnings) |

> **TL;DR for Sonoma / Sequoia:** The app will not build or run out-of-the-box on macOS 14+ without addressing the ARC, 64-bit, and `WebView` issues above.

---

## Building

1. **Open the project:**

   ```sh
   open MacOs/WinXound.xcodeproj
   ```

   Accept any project migration prompts Xcode presents.

2. **Select the target:** In the toolbar, select the **WinXound** scheme and your Mac as the destination.

3. **Build:**
   Press `⌘B` or go to **Product → Build**.

4. **Run:**
   Press `⌘R` or go to **Product → Run**. The built app is installed to `~/Applications/WinXound.app`.

---

## First Launch Configuration

On first launch, open **WinXound → Preferences** and configure paths to external tools:

- Use the **Auto-Search Paths** button — it will attempt to detect `csound`, Python, and other tools automatically.
- If auto-search misses CSound, set the path manually to `/usr/local/bin/csound`.

---

## Project Structure

```
MacOs/
├── WinXound.xcodeproj/       # Xcode project
├── scintilla/                # Bundled Scintilla editor (compiled as part of the app)
│   ├── src/                  # Scintilla engine + custom WinXound lexers
│   ├── cocoa/                # Cocoa platform layer
│   └── include/              # Public headers
├── resources/                # Bundled resources (help PDF, example .csd, error page)
├── repository/               # Built-in CSound UDO snippet library (~85 .udo/.udc files)
├── Icons/                    # App and toolbar icons
├── English.lproj/            # Main NIB / localisation files
├── wx*.h / wx*.mm            # Application source files
└── WinXound-Info.plist       # App bundle metadata
```

Scintilla is **fully bundled** — there is no separate build step for it. All `.cxx` and `.mm` files under `scintilla/` are compiled directly by the Xcode project.

---

## No Third-Party Package Manager

This project uses no CocoaPods, Carthage, or Swift Package Manager. All dependencies are either:

- **Bundled** in the repo (Scintilla), or
- **Runtime tools** that must be installed separately (CSound, Python, Lua, Cabbage).
