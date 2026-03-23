#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="${SCRIPT_DIR}/../src"

find "${SRC_DIR}" -type f \( -name "*.c" -o -name "*.h" \) -exec clang-format -i {} +
