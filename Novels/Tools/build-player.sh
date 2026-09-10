#!/bin/zsh
set -euo pipefail

if (( $# < 3 || $# > 5 )); then
  print -u2 "Usage: $0 <Remote|Embedded> <Android|iOS|Windows|macOS> <output-path> [remote-url] [--development|--test-signing]"
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
content_channel=${NOVELS_CONTENT_CHANNEL:-dev}
app_id=${NOVELS_APP_ID:-}

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
if [[ -n ${development_argument} && ${development_argument} != --development && ${development_argument} != --test-signing ]]; then
  print -u2 "Unknown option: ${development_argument}"
  exit 2
fi
if [[ ${development_argument} == --test-signing && ${target} != Android ]]; then
  print -u2 "Test signing is supported only for Android."
  exit 2
fi
if [[ ${mode} == Remote && ${remote_url} != http://* && ${remote_url} != https://* ]]; then
  print -u2 "Remote mode requires an absolute HTTP(S) URL."
  exit 2
fi
if [[ ! ${content_channel} =~ '^[a-z0-9_-]+$' ]]; then
  print -u2 "NOVELS_CONTENT_CHANNEL must be a lowercase path segment."
  exit 2
fi
if [[ ! ${app_id} =~ '^[a-z0-9-]+$' ]]; then
  print -u2 "NOVELS_APP_ID is required and must use lowercase letters, digits, and hyphens."
  exit 2
fi
app_profile_root=${somegame_root}/Projects/apps/${app_id}
app_profile=${app_profile_root}/Config/player.json
app_icon=${app_profile_root}/Assets/icon.png
if [[ ! -f ${app_profile} || ! -f ${app_icon} ]]; then
  print -u2 "Application profile '${app_id}' is incomplete: ${app_profile_root}"
  exit 3
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
if [[ ${mode} == Remote && ! -f ${content_root}/catalog/ui/Remote/${content_platform}/release.json ]]; then
  print -u2 "Fallback catalog is missing. Run: Tools/novels-tools/novels-content build catalog ${content_platform}"
  exit 3
fi

mkdir -p "${stage_root}/SomeGame/Packages" "${stage_project}" "${log_path:h}" "${output_path:h}"
rsync -a --delete "${somegame_root}/Packages/" "${stage_root}/SomeGame/Packages/"
rsync -a --delete \
  --exclude Library --exclude Temp --exclude Logs --exclude Build --exclude LocalSigning --exclude .utmp \
  "${project_root}/" "${stage_project}/"
stage_profile=${stage_project}/Assets/BuildProfiles/${app_id}
mkdir -p "${stage_profile}"
rsync -a --delete "${app_profile_root}/" "${stage_profile}/"

if [[ ${development_argument} == --test-signing ]]; then
  signing_root=${project_root}/LocalSigning
  signing_env=${signing_root}/test-signing.env
  signing_keystore=${signing_root}/test.keystore
  mkdir -p "${signing_root}"
  chmod 700 "${signing_root}"
  if [[ ! -f ${signing_env} ]]; then
    umask 077
    signing_password="novels-test-$(uuidgen | tr -d '-')"
    printf '%s\n' \
      "NOVELS_TEST_KEYSTORE_PASSWORD=${signing_password}" \
      "NOVELS_TEST_KEYALIAS=novels-test" \
      "NOVELS_TEST_KEYALIAS_PASSWORD=${signing_password}" > "${signing_env}"
  fi
  source "${signing_env}"
  : "${NOVELS_TEST_KEYSTORE_PASSWORD:?Missing test keystore password}"
  : "${NOVELS_TEST_KEYALIAS:?Missing test key alias}"
  : "${NOVELS_TEST_KEYALIAS_PASSWORD:?Missing test key alias password}"
  keytool=${UNITY_JAVA_HOME:-/Applications/Unity/Hub/Editor/6000.3.11f1/PlaybackEngines/AndroidPlayer/OpenJDK}/bin/keytool
  [[ -x ${keytool} ]] || { print -u2 "Android keytool is unavailable: ${keytool}"; exit 3 }
  if [[ ! -f ${signing_keystore} ]]; then
    "${keytool}" -genkeypair -v -keystore "${signing_keystore}" \
      -storepass "${NOVELS_TEST_KEYSTORE_PASSWORD}" -alias "${NOVELS_TEST_KEYALIAS}" \
      -keypass "${NOVELS_TEST_KEYALIAS_PASSWORD}" -keyalg RSA -keysize 2048 \
      -validity 10000 -dname "CN=SomeGame Test, OU=Development, O=SomeGame, L=Moscow, C=RU"
    chmod 600 "${signing_keystore}"
  fi
  export NOVELS_TEST_KEYSTORE_PATH=${signing_keystore}
  export NOVELS_TEST_KEYSTORE_PASSWORD NOVELS_TEST_KEYALIAS NOVELS_TEST_KEYALIAS_PASSWORD
fi

if [[ ${mode} == Embedded ]]; then
  stage_content=${stage_project}/Assets/StreamingAssets/NovelContent
  mkdir -p "${stage_content}"
  rsync -a --delete "${content_root}/" "${stage_content}/"
  execute_method=Editor.PlayerBuildAutomation.BuildEmbeddedPlayerBatch
else
  stage_catalog=${stage_project}/Assets/StreamingAssets/NovelCatalog
  mkdir -p "${stage_catalog}"
  rsync -a --delete "${content_root}/catalog/ui/" "${stage_catalog}/"
  execute_method=Editor.PlayerBuildAutomation.BuildRemotePlayerBatch
fi

unity_arguments=(
  -batchmode -quit
  -projectPath "${stage_project}"
  -buildTarget "${unity_target}"
  -executeMethod "${execute_method}"
  -playerOutput "${output_path}"
  -playerProfile "Assets/BuildProfiles/${app_id}/Config/player.json"
  -playerVersion "${version}"
  -playerBuildNumber "${build_number}"
  -logFile "${log_path}"
)
[[ ${mode} == Remote ]] && unity_arguments+=(-remoteContentBaseUrl "${remote_url}" -contentChannel "${content_channel}")
[[ ${development_argument} == --development ]] && unity_arguments+=(-developmentBuild)
[[ ${development_argument} == --test-signing ]] && unity_arguments+=(-testSigning)

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
print "Application profile: ${app_id}"
print "Build log: ${log_path}"
