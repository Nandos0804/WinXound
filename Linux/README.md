# Building WinXound on Linux

This folder contains the Linux version of WinXound.

The Linux build now supports a CMake-based workflow and builds two main parts:

1. The bundled Scintilla static library under `scintilla/gtk`.
2. The WinXound Gtkmm application under `src`.

The final runnable app is assembled with `make standalone` into the `Linux/WinXound` folder.

## Projects and Layout

- `CMakeLists.txt`: Linux CMake entry point.
- `configure.ac`: legacy autotools configure source file (`configure` is generated).
- `Makefile.am`: legacy top-level autotools rules, including the `standalone` target.
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

## Build Flow (CMake)

Build in this order:

1. Install dependencies.
2. Configure from a build directory.
3. Build with `make`.
4. Assemble runtime folder with `make standalone`.
5. Start the app from `Linux/WinXound/winxound`.

## 0. Install Dependencies

From the repository root:

```sh
./install-deps.sh
```

Optional:

- `./install-deps.sh --modern` (default)
- `./install-deps.sh --legacy`
- `./install-deps.sh -y` (non-interactive)

## 1. Configure

From the repository root:

```sh
mkdir -p build
cd build
cmake ..
```

## 2. Compile

Run:

```sh
cd build
make -j$(nproc)
```

This builds:

- the Scintilla static library used by the Linux editor
- the `winxound` executable target

## 3. Assemble the Standalone Runtime Folder

Run:

```sh
cd build
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

For a clean CMake rebuild, run:

```sh
rm -rf build
mkdir build
cd build
cmake ..
make
make standalone
```

## Legacy Autotools Workflow

If you need the historical build system:

```sh
cd Linux
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

### `cmake ..` fails on missing packages

Cause:

- One or more required development packages are missing.
- The CMake build first attempts modern packages (`gtkmm-3.0`, `vte-2.91`, `webkit2gtk`) and falls back to legacy (`gtkmm-2.4`, `vte`, `webkit-1.0`).

Fix:

- Install the matching development packages for your distribution.
- If legacy packages are unavailable, use modern package variants and then fix API differences reported by the compiler.

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
