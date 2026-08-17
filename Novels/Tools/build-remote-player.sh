#!/bin/zsh
set -euo pipefail

if (( $# < 3 )); then
  print -u2 "Usage: $0 <Android|iOS> <https://content-root> <output-path>"
  exit 2
fi

target=$1
remote_url=$2
output_path=${3:A}
script_dir=${0:A:h}
project_root=${script_dir:h}
somegame_root=${project_root:h}
unity_executable=${UNITY_EXECUTABLE:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}
stage_root=$(mktemp -d "${TMPDIR:-/tmp}/novels-remote-player.XXXXXX")
stage_project=${stage_root}/SomeGame/Novels

cleanup()
{
  if [[ -n ${stage_root} && -d ${stage_root} ]]; then
    rm -rf -- "${stage_root}"
  fi
}
trap cleanup EXIT INT TERM

mkdir -p "${stage_root}/SomeGame/Packages" "${stage_project}"
rsync -a "${somegame_root}/Packages/" "${stage_root}/SomeGame/Packages/"
rsync -a \
  --exclude Library \
  --exclude Temp \
  --exclude Logs \
  --exclude Build \
  --exclude .utmp \
  --exclude Assets/StreamingAssets/NovelTexts \
  --exclude Assets/StreamingAssets/NovelTexts.meta \
  --exclude Assets/StreamingAssets/NovelsAudio \
  --exclude Assets/StreamingAssets/NovelsAudio.meta \
  --exclude Assets/StreamingAssets/NovelsVideos \
  --exclude Assets/StreamingAssets/NovelsVideos.meta \
  --exclude Assets/StreamingAssets/Remote \
  --exclude Assets/StreamingAssets/Remote.meta \
  "${project_root}/" "${stage_project}/"

"${unity_executable}" \
  -batchmode \
  -quit \
  -projectPath "${stage_project}" \
  -buildTarget "${target}" \
  -executeMethod Editor.NovelCiValidation.BuildRemotePlayerBatch \
  -remoteContentBaseUrl "${remote_url}" \
  -playerOutput "${output_path}" \
  -logFile -
