#!/usr/bin/env bash
set -euo pipefail

if [[ "${OSTYPE:-}" != linux* ]]; then
  echo "This script supports Linux only."
  exit 1
fi

PROFILE="modern"
ASSUME_YES=false

for arg in "$@"; do
  case "$arg" in
    --modern)
      PROFILE="modern"
      ;;
    --legacy)
      PROFILE="legacy"
      ;;
    -y|--yes)
      ASSUME_YES=true
      ;;
    -h|--help)
      cat <<'EOF'
Usage: ./install-deps.sh [--modern|--legacy] [-y|--yes]

Installs build dependencies for WinXound Linux build.
Default profile is --modern.
EOF
      exit 0
      ;;
    *)
      echo "Unknown option: $arg"
      echo "Use --help for usage."
      exit 1
      ;;
  esac
done

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1"
    exit 1
  fi
}

choose_pkg_manager() {
  if command -v apt-get >/dev/null 2>&1; then
    echo "apt"
  elif command -v dnf >/dev/null 2>&1; then
    echo "dnf"
  elif command -v pacman >/dev/null 2>&1; then
    echo "pacman"
  elif command -v zypper >/dev/null 2>&1; then
    echo "zypper"
  else
    echo ""
  fi
}

PKG_MANAGER="$(choose_pkg_manager)"
if [[ -z "$PKG_MANAGER" ]]; then
  echo "Unsupported distribution: no apt, dnf, pacman, or zypper found."
  exit 1
fi

run_cmd() {
  echo "+ $*"
  "$@"
}

run_root_cmd() {
  if [[ $EUID -eq 0 ]]; then
    run_cmd "$@"
  else
    if command -v sudo >/dev/null 2>&1; then
      run_cmd sudo "$@"
    else
      echo "Need root privileges, but sudo is not available."
      exit 1
    fi
  fi
}

install_apt() {
  require_cmd apt-get

  local common=(
    build-essential
    cmake
    pkg-config
    autoconf
    automake
    libtool
  )

  local modern=(
    libgtkmm-3.0-dev
    libvte-2.91-dev
  )

  local legacy=(
    libgtkmm-2.4-dev
    libvte-dev
    libwebkit-dev
  )

  local webkit_modern=""
  if apt-cache show libwebkit2gtk-4.1-dev >/dev/null 2>&1; then
    webkit_modern="libwebkit2gtk-4.1-dev"
  elif apt-cache show libwebkit2gtk-4.0-dev >/dev/null 2>&1; then
    webkit_modern="libwebkit2gtk-4.0-dev"
  fi

  run_root_cmd apt-get update

  if [[ "$PROFILE" == "modern" ]]; then
    if [[ -z "$webkit_modern" ]]; then
      echo "Could not find libwebkit2gtk-4.1-dev or libwebkit2gtk-4.0-dev in apt repositories."
      echo "Install one of them manually, then re-run this script."
      exit 1
    fi
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd apt-get install -y "${common[@]}" "${modern[@]}" "$webkit_modern"
    else
      run_root_cmd apt-get install "${common[@]}" "${modern[@]}" "$webkit_modern"
    fi
  else
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd apt-get install -y "${common[@]}" "${legacy[@]}"
    else
      run_root_cmd apt-get install "${common[@]}" "${legacy[@]}"
    fi
  fi
}

install_dnf() {
  require_cmd dnf

  local common=(
    gcc
    gcc-c++
    make
    cmake
    pkgconf-pkg-config
    autoconf
    automake
    libtool
  )

  local modern=(
    gtkmm30-devel
    vte291-devel
  )

  local legacy=(
    gtkmm24-devel
    vte-devel
    webkitgtk3-devel
  )

  local webkit_modern=""
  if dnf info webkit2gtk4.1-devel >/dev/null 2>&1; then
    webkit_modern="webkit2gtk4.1-devel"
  elif dnf info webkit2gtk3-devel >/dev/null 2>&1; then
    webkit_modern="webkit2gtk3-devel"
  fi

  if [[ "$PROFILE" == "modern" ]]; then
    if [[ -z "$webkit_modern" ]]; then
      echo "Could not find webkit2gtk4.1-devel/webkit2gtk3-devel in dnf repositories."
      exit 1
    fi
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd dnf install -y "${common[@]}" "${modern[@]}" "$webkit_modern"
    else
      run_root_cmd dnf install "${common[@]}" "${modern[@]}" "$webkit_modern"
    fi
  else
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd dnf install -y "${common[@]}" "${legacy[@]}"
    else
      run_root_cmd dnf install "${common[@]}" "${legacy[@]}"
    fi
  fi
}

install_pacman() {
  require_cmd pacman

  local common=(
    base-devel
    cmake
    pkgconf
    autoconf
    automake
    libtool
  )

  local modern=(
    gtkmm3
    vte3
    webkit2gtk
  )

  local legacy=(
    gtkmm
    vte
    webkitgtk
  )

  if [[ "$PROFILE" == "modern" ]]; then
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd pacman -Syu --needed --noconfirm "${common[@]}" "${modern[@]}"
    else
      run_root_cmd pacman -Syu --needed "${common[@]}" "${modern[@]}"
    fi
  else
    if [[ "$ASSUME_YES" == true ]]; then
      run_root_cmd pacman -Syu --needed --noconfirm "${common[@]}" "${legacy[@]}"
    else
      run_root_cmd pacman -Syu --needed "${common[@]}" "${legacy[@]}"
    fi
  fi
}

install_zypper() {
  require_cmd zypper

  local common=(
    gcc
    gcc-c++
    make
    cmake
    pkg-config
    autoconf
    automake
    libtool
  )

  local modern=(
    gtkmm3-devel
    vte-devel
    webkit2gtk3-devel
  )

  local legacy=(
    gtkmm2-devel
    vte-devel
    webkitgtk3-devel
  )

  if [[ "$ASSUME_YES" == true ]]; then
    run_root_cmd zypper install -y "${common[@]}" $([[ "$PROFILE" == "modern" ]] && printf '%s ' "${modern[@]}" || printf '%s ' "${legacy[@]}")
  else
    run_root_cmd zypper install "${common[@]}" $([[ "$PROFILE" == "modern" ]] && printf '%s ' "${modern[@]}" || printf '%s ' "${legacy[@]}")
  fi
}

echo "Installing WinXound dependencies"
echo "- profile: $PROFILE"
echo "- package manager: $PKG_MANAGER"

case "$PKG_MANAGER" in
  apt)
    install_apt
    ;;
  dnf)
    install_dnf
    ;;
  pacman)
    install_pacman
    ;;
  zypper)
    install_zypper
    ;;
  *)
    echo "Internal error: unknown package manager $PKG_MANAGER"
    exit 1
    ;;
esac

echo
echo "Dependencies installed."
echo "Next steps:"
echo "  mkdir -p build"
echo "  cd build"
echo "  cmake .."
echo "  make -j\$(nproc)"
