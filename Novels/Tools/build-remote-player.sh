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
build_cache_root=${NOVELS_REMOTE_PLAYER_CACHE_ROOT:-${project_root}/Library/RemotePlayerBuild}
stage_root=${build_cache_root}/${target}
stage_project=${stage_root}/SomeGame/Novels
log_path=${NOVELS_REMOTE_PLAYER_LOG:-${project_root}/Build/Logs/remote-player-${target}.log}

case ${target} in
  Android|iOS) ;;
  *)
    print -u2 "Target must be Android or iOS: ${target}"
    exit 2
    ;;
esac

if [[ ${remote_url} != http://* && ${remote_url} != https://* ]]; then
  print -u2 "Remote content root must be an absolute HTTP(S) URL."
  exit 2
fi
if [[ ! -x ${unity_executable} ]]; then
  print -u2 "Unity executable is unavailable: ${unity_executable}"
  exit 3
fi
if [[ ! -f ${project_root}/Packages/manifest.json || ! -d ${somegame_root}/Packages ]]; then
  print -u2 "Unity package sources are incomplete."
  exit 3
fi

mkdir -p "${stage_root}/SomeGame/Packages" "${stage_project}" "${log_path:h}"
rsync -a --delete \
  "${somegame_root}/Packages/" "${stage_root}/SomeGame/Packages/"
rsync -a \
  --delete \
  --exclude Library \
  --exclude Temp \
  --exclude Logs \
  --exclude Build \
  --exclude .utmp \
  --exclude Assets/RemoteAssets \
  --exclude Assets/RemoteAssets.meta \
  --exclude Assets/StreamingAssets/noveltexts \
  --exclude Assets/StreamingAssets/noveltexts.meta \
  --exclude Assets/StreamingAssets/novelsaudio \
  --exclude Assets/StreamingAssets/novelsaudio.meta \
  --exclude Assets/StreamingAssets/novelsvideos \
  --exclude Assets/StreamingAssets/novelsvideos.meta \
  --exclude Assets/StreamingAssets/Remote \
  --exclude Assets/StreamingAssets/Remote.meta \
  "${project_root}/" "${stage_project}/"

set +e
"${unity_executable}" \
  -batchmode \
  -quit \
  -projectPath "${stage_project}" \
  -buildTarget "${target}" \
  -executeMethod Editor.NovelCiValidation.BuildRemotePlayerBatch \
  -remoteContentBaseUrl "${remote_url}" \
  -playerOutput "${output_path}" \
  -logFile "${log_path}"
status=$?
set -e

if (( status != 0 )); then
  print -u2 "Remote Player build failed. Log: ${log_path}"
  tail -n 200 "${log_path}" >&2 || true
  exit ${status}
fi

print "Remote Player build completed: ${output_path}"
print "Reusable staging project: ${stage_project}"
print "Build log: ${log_path}"
