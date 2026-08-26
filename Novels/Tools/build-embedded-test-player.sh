#!/bin/zsh
set -euo pipefail

if (( $# < 2 || $# > 3 )); then
  print -u2 "Usage: $0 <Android|iOS|Windows|macOS> <output-path> [--development]"
  exit 2
fi
script_dir=${0:A:h}
exec "${script_dir}/build-player.sh" Embedded "$1" "$2" "" "${3:-}"
