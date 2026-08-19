#!/bin/zsh
set -euo pipefail

script_dir=${0:A:h}
project_root=${script_dir:h}
default_config=${script_dir}/release-content.env

usage() {
  cat <<'EOF'
Usage: Tools/release-novel-content.sh [options]

By default, builds and validates Android/iOS content locally.

Options:
  --config <path>          Load configuration file (default: Tools/release-content.env).
  --local-only             Never upload, verify remote content, or build Players.
  --skip-content-build     Use the existing Build/NovelContent/ServerRoot.
  --server-url <https-url> Public URL corresponding to ServerRoot.
  --upload                 Upload through rsync using the configured target.
  --no-upload              Disable upload even when enabled by configuration.
  --upload-target <target> rsync destination; also enables upload.
  --build-android          Build an Android Player after remote verification.
  --build-ios              Build an iOS Xcode project after remote verification.
  --android-output <path>  Android output path.
  --ios-output <path>      iOS Xcode-project output directory.
  --unity <path>           Unity executable path.
  -h, --help               Show this help.

Upload order: immutable Files, versioned bundles, deployment.json, release.json files.
EOF
}

fail() {
  print -u2 "Release failed: $*"
  exit 2
}

stage() {
  print ""
  print "==> $*"
}

parse_bool() {
  case ${(L)1} in
    1|true|yes|on) print 1 ;;
    0|false|no|off|'') print 0 ;;
    *) fail "Invalid boolean value '$1'." ;;
  esac
}

config_path=${default_config}
arguments=("$@")
for (( index = 1; index <= ${#arguments}; index++ )); do
  if [[ ${arguments[index]} == --config ]]; then
    (( index < ${#arguments} )) || fail "--config requires a path."
    config_path=${arguments[index + 1]:A}
    break
  fi
done

if [[ -f ${config_path} ]]; then
  stage "Load configuration ${config_path}"
  source ${config_path}
elif [[ ${config_path} != ${default_config} ]]; then
  fail "Configuration file does not exist: ${config_path}"
fi

unity_executable=${NOVELS_RELEASE_UNITY:-${UNITY_EXECUTABLE:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}}
server_url=${NOVELS_RELEASE_SERVER_URL:-}
upload_target=${NOVELS_RELEASE_UPLOAD_TARGET:-}
upload_enabled=$(parse_bool "${NOVELS_RELEASE_UPLOAD_ENABLED:-0}")
build_android=$(parse_bool "${NOVELS_RELEASE_BUILD_ANDROID:-0}")
build_ios=$(parse_bool "${NOVELS_RELEASE_BUILD_IOS:-0}")
android_output=${NOVELS_RELEASE_ANDROID_OUTPUT:-${project_root}/Build/Players/Novels.apk}
ios_output=${NOVELS_RELEASE_IOS_OUTPUT:-${project_root}/Build/Players/iOS}
local_only=0
skip_content_build=0

while (( $# > 0 )); do
  case $1 in
    --config)
      (( $# >= 2 )) || fail "--config requires a path."
      shift 2
      ;;
    --local-only)
      local_only=1
      shift
      ;;
    --skip-content-build)
      skip_content_build=1
      shift
      ;;
    --server-url)
      (( $# >= 2 )) || fail "--server-url requires a URL."
      server_url=$2
      shift 2
      ;;
    --upload)
      upload_enabled=1
      shift
      ;;
    --no-upload)
      upload_enabled=0
      shift
      ;;
    --upload-target)
      (( $# >= 2 )) || fail "--upload-target requires a destination."
      upload_target=$2
      upload_enabled=1
      shift 2
      ;;
    --build-android)
      build_android=1
      shift
      ;;
    --build-ios)
      build_ios=1
      shift
      ;;
    --android-output)
      (( $# >= 2 )) || fail "--android-output requires a path."
      android_output=$2
      shift 2
      ;;
    --ios-output)
      (( $# >= 2 )) || fail "--ios-output requires a path."
      ios_output=$2
      shift 2
      ;;
    --unity)
      (( $# >= 2 )) || fail "--unity requires a path."
      unity_executable=$2
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      fail "Unknown option '$1'. Run with --help."
      ;;
  esac
done

unity_executable=${unity_executable:A}
if [[ ${android_output} != /* ]]; then
  android_output=${project_root}/${android_output}
fi
if [[ ${ios_output} != /* ]]; then
  ios_output=${project_root}/${ios_output}
fi
android_output=${android_output:A}
ios_output=${ios_output:A}
server_url=${server_url%/}
server_root=${project_root}/Build/NovelContent/ServerRoot
log_root=${project_root}/Build/Logs
content_log=${log_root}/content-release.log

if (( local_only )); then
  upload_enabled=0
  build_android=0
  build_ios=0
  server_url=''
fi

[[ -x ${unity_executable} ]] || fail "Unity executable is unavailable: ${unity_executable}"
export UNITY_EXECUTABLE=${unity_executable}
[[ -f ${project_root}/Packages/manifest.json ]] || fail "Unity project is invalid: ${project_root}"
command -v ruby >/dev/null || fail "Ruby is required."

if (( upload_enabled )); then
  command -v rsync >/dev/null || fail "rsync is required for upload."
  [[ -n ${upload_target} ]] || fail "--upload requires --upload-target."
  [[ -n ${server_url} ]] || fail "--upload requires --server-url."
fi
if (( build_android || build_ios )); then
  [[ -n ${server_url} ]] || fail "Player builds require --server-url."
fi
if [[ -n ${server_url} && ${server_url} != http://* && ${server_url} != https://* ]]; then
  fail "Server URL must be absolute HTTP(S): ${server_url}"
fi

mkdir -p ${log_root}

if (( ! skip_content_build )); then
  [[ ! -e ${project_root}/Temp/UnityLockfile ]] \
    || fail "Close the Unity Editor before starting the batch content build."
  stage "Build and validate Android/iOS content"
  set +e
  ${unity_executable} \
    -batchmode \
    -quit \
    -projectPath ${project_root} \
    -executeMethod Editor.NovelCiValidation.BuildAndValidateContentBatch \
    -logFile ${content_log}
  content_status=$?
  set -e
  if (( content_status != 0 )); then
    tail -n 200 ${content_log} >&2 || true
    fail "Unity content build failed. Log: ${content_log}"
  fi
else
  stage "Use existing ServerRoot"
fi

stage "Validate local ServerRoot contract"
deployment_path=${server_root}/deployment.json
[[ -f ${deployment_path} ]] || fail "deployment.json is missing: ${deployment_path}"
[[ -f ${server_root}/Remote/Android/release.json ]] \
  || fail "Android release.json is missing."
[[ -f ${server_root}/Remote/iOS/release.json ]] \
  || fail "iOS release.json is missing."
deployment_id=$(ruby -rjson -e '
  document = JSON.parse(File.read(ARGV.fetch(0)))
  platforms = (document["platforms"] || []).map { |value| value["platform"] }
  abort "deployment.json must contain Android and iOS" \
    unless ["Android", "iOS"].all? { |value| platforms.include?(value) }
  puts document.fetch("deploymentId")
' ${deployment_path})
[[ -n ${deployment_id} ]] || fail "deployment.json has an empty deploymentId."
print "Local deployment: ${deployment_id}"

if (( upload_enabled )); then
  stage "Upload immutable content files"
  rsync -a ${server_root}/Files/ "${upload_target}/Files/"

  stage "Upload versioned platform bundles"
  rsync -a --exclude release.json ${server_root}/Remote/ "${upload_target}/Remote/"

  stage "Upload deployment manifest"
  rsync -a ${deployment_path} "${upload_target}/deployment.json"

  stage "Activate Android release"
  rsync -a \
    ${server_root}/Remote/Android/release.json \
    "${upload_target}/Remote/Android/release.json"

  stage "Activate iOS release"
  rsync -a \
    ${server_root}/Remote/iOS/release.json \
    "${upload_target}/Remote/iOS/release.json"
fi

if [[ -n ${server_url} ]]; then
  stage "Verify uploaded Android content"
  ruby ${script_dir}/verify-server-root.rb ${server_url} Android ${deployment_id}

  stage "Verify uploaded iOS content"
  ruby ${script_dir}/verify-server-root.rb ${server_url} iOS ${deployment_id}
fi

if (( build_android )); then
  stage "Build Android Player"
  zsh ${script_dir}/build-remote-player.sh \
    Android \
    ${server_url} \
    ${android_output}
fi

if (( build_ios )); then
  stage "Build iOS Xcode project"
  zsh ${script_dir}/build-remote-player.sh \
    iOS \
    ${server_url} \
    ${ios_output}
fi

stage "Release process completed"
print "Deployment: ${deployment_id}"
print "ServerRoot: ${server_root}"
print "Content log: ${content_log}"
if [[ -n ${server_url} ]]; then
  print "Remote root: ${server_url}"
fi
