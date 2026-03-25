# Building WinXound on Windows

This folder contains the Windows version of WinXound.

The Windows build has two parts:

1. A native Scintilla build that produces `SciLexer.dll`.
2. A C# solution that builds the editor, the text editor wrapper, and the realtime console helper.

## Projects

- `WinXound_Net.sln`: main Windows solution.
- `WinXound_Net/`: main WinForms application.
- `ScintillaTextEditor/`: managed wrapper/control around Scintilla.
- `RTConsole/`: helper executable used for realtime console output.
- `scintilla211_for_winxound/`: native Scintilla source tree used to build `SciLexer.dll`.

## Prerequisites

- Windows 10 or Windows 11.
- Visual Studio or Visual Studio Build Tools with:
  - .NET desktop development
  - C++ build tools
- A Developer Command Prompt for Visual Studio if you want to build `SciLexer.dll` from the command line.

Notes:

- The C# projects target .NET Framework 4.0.
- The solution was originally created with Visual C# Express 2010.
- On modern Visual Studio versions you may be prompted to retarget or upgrade project metadata locally.

## Runtime Layout

The built application expects these files and folders under `Windows/bin`:

- `WinXound_Net.exe`
- `ScintillaTextEditor.dll`
- `SciLexer.dll`
- `Utility/WinXound_RTConsole.exe`
- `Utility/opcodes.txt`
- `CodeRepository/`
- `Help/`
- `Icons/`
- `Settings/`
- `History.txt`
- `ReadMe.txt`

The repository intentionally keeps the resource files under `Windows/bin`, but generated binaries are not committed.

## Build Order

Build in this order:

1. Build `SciLexer.dll`.
2. Build `WinXound_Net.sln`.
3. Confirm the output files are present in `Windows/bin`.

## 1. Build SciLexer.dll

Open a Visual Studio Developer Command Prompt and run:

```bat
cd Windows\scintilla211_for_winxound\win32
nmake -f scintilla.mak
copy ..\bin\SciLexer.dll ..\..\bin\SciLexer.dll
```

This builds `SciLexer.dll` inside `Windows/scintilla211_for_winxound/bin` and copies it into `Windows/bin`, which is where WinXound loads it at runtime.

Why this is required:

- `ScintillaTextEditor` calls `LoadLibrary("SciLexer.dll")`.
- `WinXound_Net` also reads the Scintilla version from `Windows/bin/SciLexer.dll`.

## 2. Build the C# Solution

Open `Windows/WinXound_Net.sln` in Visual Studio.

Recommended configuration:

- `Configuration`: `Debug` or `Release`
- `Platform`: `x86`

Then build the full solution.

Expected outputs:

- `WinXound_Net` builds to `Windows/bin`
- `ScintillaTextEditor` builds to `Windows/bin`
- `RTConsole` builds to `Windows/bin/Utility`

If you prefer the command line and have MSBuild available, the equivalent command is:

```bat
msbuild Windows\WinXound_Net.sln /p:Configuration=Release /p:Platform=x86
```

## 3. Verify the Output Tree

After a successful build, verify that these files exist:

```text
Windows/bin/WinXound_Net.exe
Windows/bin/ScintillaTextEditor.dll
Windows/bin/SciLexer.dll
Windows/bin/Utility/WinXound_RTConsole.exe
```

The existing resource folders under `Windows/bin` should still be present.

## Run

Start the application from:

```text
Windows/bin/WinXound_Net.exe
```

At first launch, verify the compiler paths from the WinXound settings UI if you want Csound, Python, or Lua integration to work.

## Troubleshooting

### `SciLexer.dll` missing

Symptoms:

- The editor control does not initialize.
- Syntax highlighting/editor startup fails.

Fix:

- Rebuild `SciLexer.dll` from `scintilla211_for_winxound/win32`.
- Copy the resulting `SciLexer.dll` into `Windows/bin`.

### `WinXound_RTConsole.exe` missing

Symptoms:

- Realtime console output features fail.

Fix:

- Rebuild the full solution.
- Confirm `Windows/bin/Utility/WinXound_RTConsole.exe` exists.

### `opcodes.txt` missing

Symptoms:

- Opcode completion or related features may fail or regenerate the file.

Fix:

- Keep `Windows/bin/Utility/opcodes.txt` in place.
- If it is missing, WinXound can recreate it from embedded resources on startup.

### Modern Visual Studio cannot target .NET Framework 4.0

Fix:

- Install the required .NET Framework targeting pack if available in your environment.
- If that is not possible, retarget the projects locally and test before committing any project-file changes.

## Clean Rebuild

Generated Windows build outputs are ignored by git. If you want a clean rebuild, remove generated contents from:

- `Windows/scintilla211_for_winxound/bin`
- `Windows/ScintillaTextEditor/bin`
- `Windows/**/obj`
- generated binaries under `Windows/bin`

Then rebuild using the steps above.
