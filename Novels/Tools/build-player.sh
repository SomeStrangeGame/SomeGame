#!/bin/zsh
set -euo pipefail

if (( $# < 3 || $# > 5 )); then
  print -u2 "Usage: $0 <Remote|Embedded> <Android|iOS|Windows|macOS> <output-path> [remote-url] [--development]"
  exit 2
fi

mode=$1
target=$2
output_path=${3:A}
remote_url=${4:-}
development_argument=${5:-}
script_dir=${0:A:h}
project_root=${script_dir:h}
somegame_root=${project_root:h}
content_root=${project_root}/Build/LocalContent
unity_executable=${UNITY_EXECUTABLE:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}
version=${NOVELS_PLAYER_VERSION:-$(date -u +%Y.%m.%d)}
build_number=${NOVELS_PLAYER_BUILD_NUMBER:-$(( ($(date -u +%s) - 1577836800) / 60 ))}

case ${target} in
  Android) unity_target=Android ; content_platform=android ;;
  iOS) unity_target=iOS ; content_platform=ios ;;
  Windows) unity_target=Win64 ; content_platform=windows ;;
  macOS) unity_target=StandaloneOSX ; content_platform=editor ;;
  *) print -u2 "Unsupported target: ${target}"; exit 2 ;;
esac
case ${mode} in
  Remote|Embedded) ;;
  *) print -u2 "Mode must be Remote or Embedded: ${mode}"; exit 2 ;;
esac
if [[ -n ${development_argument} && ${development_argument} != --development ]]; then
  print -u2 "Unknown option: ${development_argument}"
  exit 2
fi
if [[ ${mode} == Remote && ${remote_url} != http://* && ${remote_url} != https://* ]]; then
  print -u2 "Remote mode requires an absolute HTTP(S) URL."
  exit 2
fi

stage_root=${project_root}/Library/PlayerBuild/${mode}/${target}
stage_project=${stage_root}/SomeGame/Novels
log_path=${NOVELS_PLAYER_LOG:-${project_root}/Build/Logs/player-${target}-${mode}.log}
[[ -x ${unity_executable} ]] || { print -u2 "Unity executable is unavailable: ${unity_executable}"; exit 3 }
[[ -f ${project_root}/Packages/manifest.json && -d ${somegame_root}/Packages ]] || {
  print -u2 "Unity package sources are incomplete."
  exit 3
}

if [[ ${mode} == Embedded && ! -f ${content_root}/catalog/registry/catalog.json ]]; then
  print -u2 "Embedded content is missing. Run: Tools/novels-tools/novels-content build all ${content_platform}"
  exit 3
fi

mkdir -p "${stage_root}/SomeGame/Packages" "${stage_project}" "${log_path:h}" "${output_path:h}"
rsync -a --delete "${somegame_root}/Packages/" "${stage_root}/SomeGame/Packages/"
rsync -a --delete \
  --exclude Library --exclude Temp --exclude Logs --exclude Build --exclude .utmp \
  "${project_root}/" "${stage_project}/"

if [[ ${mode} == Embedded ]]; then
  stage_content=${stage_project}/Assets/StreamingAssets/NovelContent
  mkdir -p "${stage_content}"
  rsync -a --delete "${content_root}/" "${stage_content}/"
  execute_method=Editor.PlayerBuildAutomation.BuildEmbeddedPlayerBatch
else
  execute_method=Editor.PlayerBuildAutomation.BuildRemotePlayerBatch
fi

unity_arguments=(
  -batchmode -quit
  -projectPath "${stage_project}"
  -buildTarget "${unity_target}"
  -executeMethod "${execute_method}"
  -playerOutput "${output_path}"
  -playerVersion "${version}"
  -playerBuildNumber "${build_number}"
  -logFile "${log_path}"
)
[[ ${mode} == Remote ]] && unity_arguments+=(-remoteContentBaseUrl "${remote_url}")
[[ ${development_argument} == --development ]] && unity_arguments+=(-developmentBuild)

set +e
"${unity_executable}" "${unity_arguments[@]}"
build_status=$?
set -e
if (( build_status != 0 )); then
  print -u2 "${mode} ${target} Player build failed. Log: ${log_path}"
  tail -n 200 "${log_path}" >&2 || true
  exit ${build_status}
fi

print "${mode} ${target} Player completed: ${output_path}"
print "Version: ${version} (${build_number})"
print "Build log: ${log_path}"
