# Building WinXound on Linux

This folder contains the Linux version of WinXound built with CMake and C++17.

The build compiles two components:

1. A bundled Scintilla static library from `scintilla/`.
2. The WinXound Gtkmm application from `src/`.

The final runnable app is assembled with `make standalone` into `WinXound/`.

## Layout

| Path              | Description                                        |
| ----------------- | -------------------------------------------------- |
| `CMakeLists.txt`  | CMake entry point                                  |
| `install-deps.sh` | Dependency installer (apt / dnf / pacman / zypper) |
| `src/`            | WinXound application source (C++17, Gtkmm)         |
| `scintilla/`      | Bundled Scintilla editor component                 |
| `WinXound/`       | Runtime folder — binary + resources                |
| `scripts/`        | `format.sh` and `lint.sh` helpers                  |

## Prerequisites

- `g++` (C++17), `make`, `cmake >= 3.16`, `pkg-config`
- One of the supported dependency profiles (auto-detected):

| Profile              | Packages                                                              |
| -------------------- | --------------------------------------------------------------------- |
| **modern** (default) | `gtkmm-3.0 >= 3.22`, `vte-2.91`, `webkit2gtk-4.1` or `webkit2gtk-4.0` |
| **legacy**           | `gtkmm-2.4 >= 2.12`, `vte`, `webkit-1.0`                              |

- Csound installed for runtime audio features.

## Build

### 1. Install dependencies

```sh
./install-deps.sh          # modern profile, interactive
./install-deps.sh --legacy # legacy profile
./install-deps.sh -y       # non-interactive
```

Supported distributions: Debian/Ubuntu (`apt`), Fedora/RHEL (`dnf`), Arch (`pacman`), openSUSE (`zypper`).

### 2. Configure

```sh
mkdir -p build && cd build
cmake ..
```

To force a specific profile:

```sh
cmake -DWINXOUND_DEP_PROFILE=modern ..
cmake -DWINXOUND_DEP_PROFILE=legacy ..
```

### 3. Compile

```sh
make -j$(nproc)
```

### 4. Assemble the runtime folder

```sh
make standalone
```

Copies into `WinXound/`:

- `winxound` — the compiled binary
- `src/*.ui` — GTK UI definition files
- `src/*.png` — icons
- `src/opcodes.txt` — Csound opcode list

### 5. Run

```sh
./WinXound/winxound
```

The `WinXound/` folder must remain in a location with read and write permission.

## Additional make targets

| Target        | Description                              |
| ------------- | ---------------------------------------- |
| `make format` | Auto-format sources with `clang-format`  |
| `make lint`   | Lint check with `clang-format --dry-run` |

These targets are only available if `clang-format` is installed.

## Runtime layout

```bash
WinXound/
  winxound        ← binary
  src/            ← UI files, opcodes.txt, icons
  Help/
  Repository/
  Cabbage/
```

## Clean rebuild

```sh
rm -rf build
mkdir build && cd build
cmake ..
make -j$(nproc)
make standalone
```

## Troubleshooting

**`cmake ..` fails with "No supported dependency profile found"**
Install the modern or legacy package set for your distribution using `install-deps.sh`, or pass `-DWINXOUND_DEP_PROFILE=modern|legacy` explicitly.

**App does not start after `make standalone`**
Run `make standalone` again and launch from `WinXound/winxound` — do not copy just the binary.

**`clang-format` not found**
The `format` and `lint` targets are skipped silently. Install `clang-format` to enable them.
