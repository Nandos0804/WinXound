# Building WinXound on Linux

This folder contains the Linux version of WinXound.

The Linux build uses an autotools-based workflow and builds two main parts:

1. The bundled Scintilla static library under `scintilla/gtk`.
2. The WinXound Gtkmm application under `src`.

The final runnable app is assembled with `make standalone` into the `Linux/WinXound` folder.

## Projects and Layout

- `configure.ac`: autotools configure source file (`configure` is generated).
- `Makefile.am`: top-level build rules, including the `standalone` target.
- `src/`: WinXound Gtkmm application source.
- `scintilla/`: bundled Scintilla source used by the editor.
- `WinXound/`: runtime folder containing resources and the standalone app layout.

## Prerequisites

- A Linux distribution with development tools installed.
- `g++`, `make`, `pkg-config`, and standard autotools utilities.
- Development packages required by `configure.ac`:
  - `gtkmm-2.4 >= 2.12`
  - `vte`
  - `webkit-1.0`
- Csound installed if you want to use WinXound with Csound.
- Optional: Python if you want Python integration.

Notes:

- This is an older Gtkmm 2.x codebase, so package names and availability depend on the Linux distribution.
- On modern systems, these libraries may require compatibility packages or manual package-name mapping.

## Build Flow

Build in this order:

1. Run `autogen.sh` (generates autotools files and runs `configure`).
2. Run `make`.
3. Run `make standalone`.
4. Start the app from `Linux/WinXound/winxound`.

## 1. Generate and Configure

From the repository root:

```sh
cd Linux
./autogen.sh
```

This script regenerates autotools files from `configure.ac` and then runs `configure`.

## 2. Compile

Run:

```sh
cd Linux
make
```

This builds:

- the Scintilla static library used by the Linux editor
- the `src/winxound` executable

## 3. Assemble the Standalone Runtime Folder

Run:

```sh
cd Linux
make standalone
```

This target copies the compiled application and required UI/resource files into the runtime folder layout under `Linux/WinXound`.

The standalone target copies at least:

- `src/winxound` to `WinXound/winxound`
- `src/*.ui` to `WinXound/src`
- `src/*.png` to `WinXound/src`
- `src/opcodes.txt` to `WinXound/src/opcodes.txt`

## 4. Run

Start the application with:

```sh
cd Linux
./WinXound/winxound
```

The `Linux/WinXound` folder should remain in a location where the user has read and write permission.

## Runtime Layout

The Linux runtime folder in this repository is:

- `Linux/WinXound/Help`
- `Linux/WinXound/Repository`
- `Linux/WinXound/Settings`
- `Linux/WinXound/Cabbage`
- `Linux/WinXound/src`

These are runtime assets and should remain tracked.

## Clean Rebuild

Generated Linux build outputs are ignored by git. For a clean rebuild, run:

```sh
cd Linux
make distclean || true
./autogen.sh
make
make standalone
```

If you want to remove generated files manually, the main rebuildable paths are:

- `Linux/autom4te.cache`
- `Linux/aclocal.m4`
- `Linux/configure`
- `Linux/config.h.in`
- `Linux/config.guess`
- `Linux/config.sub`
- `Linux/depcomp`
- `Linux/install-sh`
- `Linux/ltmain.sh`
- `Linux/missing`
- `Linux/Makefile.in`
- `Linux/config.h`
- `Linux/config.log`
- `Linux/config.status`
- `Linux/libtool`
- `Linux/Makefile`
- `Linux/src/Makefile.in`
- `Linux/src/Makefile`
- `Linux/src/.deps`
- `Linux/scintilla/.deps`
- `Linux/scintilla/bin`

## Troubleshooting

### `./configure` fails on missing packages

Cause:

- One or more of `gtkmm-2.4`, `vte`, or `webkit-1.0` development packages are missing.

Fix:

- Install the matching development packages for your distribution.
- If your distribution no longer ships these versions, use compatibility packages or build in an environment that still provides Gtkmm 2.x and WebKit 1.0.

### `make standalone` succeeds but the app does not start correctly

Cause:

- The standalone runtime folder is incomplete or moved incorrectly.

Fix:

- Re-run `make standalone`.
- Launch the executable from the `Linux/WinXound` layout instead of copying only the binary.

### `autogen.sh` fails

Cause:

- Missing autotools utilities such as `autoconf`, `automake`, `aclocal`, or `libtool`.

Fix:

- Install the missing autotools packages and retry.

## Legacy Notes

The original historical Linux instructions are still available in `Linux/README`. This `Linux/README.md` file is the maintained build guide for current contributors.