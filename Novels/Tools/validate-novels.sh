#!/bin/zsh
set -euo pipefail

script_dir=${0:A:h}
project_root=${script_dir:h}
unity_executable=${UNITY_EXECUTABLE:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}
mode=${1:-validate}

case ${mode} in
  validate)
    method=Editor.NovelCiValidation.ValidateExistingContentBatch
    ;;
  content)
    method=Editor.NovelCiValidation.BuildAndValidateContentBatch
    ;;
  *)
    print -u2 "Usage: $0 [validate|content]"
    exit 2
    ;;
esac

"${unity_executable}" \
  -batchmode \
  -quit \
  -projectPath "${project_root}" \
  -executeMethod "${method}" \
  -logFile -
