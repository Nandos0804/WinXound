#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="${SCRIPT_DIR}/../src"

failed=0

while IFS= read -r -d '' file; do
    if ! clang-format --dry-run --Werror "${file}" >/dev/null 2>&1; then
        echo "Formatting check failed: ${file}"
        failed=1
    fi
done < <(find "${SRC_DIR}" -type f \( -name "*.c" -o -name "*.h" \) -print0)

if [[ "${failed}" -ne 0 ]]; then
    exit 1
fi

echo "All files are properly formatted."
