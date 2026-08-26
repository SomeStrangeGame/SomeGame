#!/bin/zsh
set -euo pipefail

if (( $# < 3 || $# > 4 )); then
  print -u2 "Usage: $0 <Android|iOS|Windows|macOS> <https://content-root> <output-path> [--development]"
  exit 2
fi
script_dir=${0:A:h}
exec "${script_dir}/build-player.sh" Remote "$1" "$3" "$2" "${4:-}"
