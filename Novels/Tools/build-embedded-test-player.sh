#!/bin/zsh
set -euo pipefail

script_dir=${0:A:h}
project_root=${script_dir:h}
somegame_root=${project_root:h}
unity_executable=${UNITY_EXECUTABLE:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}
stage_root=${NOVELS_EMBEDDED_PLAYER_CACHE_ROOT:-${project_root}/Library/EmbeddedTestPlayerBuild}
stage_project=${stage_root}/SomeGame/Novels
output_path=${1:-${project_root}/Build/Players/Novels-embedded-test.apk}
log_path=${NOVELS_EMBEDDED_PLAYER_LOG:-${project_root}/Build/Logs/embedded-test-player-Android.log}

[[ -x ${unity_executable} ]] || {
  print -u2 "Unity executable is unavailable: ${unity_executable}"
  exit 3
}
[[ -f ${project_root}/Assets/StreamingAssets/Remote/Android/release.json ]] || {
  print -u2 "Android StreamingAssets release is missing. Build content first."
  exit 3
}

output_path=${output_path:A}
mkdir -p "${stage_root}/SomeGame/Packages" "${stage_project}" "${log_path:h}"
rsync -a --delete \
  "${somegame_root}/Packages/" "${stage_root}/SomeGame/Packages/"
rsync -a --delete \
  --exclude Library \
  --exclude Temp \
  --exclude Logs \
  --exclude Build \
  --exclude .utmp \
  "${project_root}/" "${stage_project}/"

set +e
"${unity_executable}" \
  -batchmode \
  -quit \
  -projectPath "${stage_project}" \
  -buildTarget Android \
  -executeMethod Editor.NovelCiValidation.BuildEmbeddedTestPlayerBatch \
  -playerOutput "${output_path}" \
  -logFile "${log_path}"
status=$?
set -e

if (( status != 0 )); then
  print -u2 "Embedded test Player build failed. Log: ${log_path}"
  tail -n 200 "${log_path}" >&2 || true
  exit ${status}
fi

print "Embedded test Player build completed: ${output_path}"
print "Reusable staging project: ${stage_project}"
print "Build log: ${log_path}"
