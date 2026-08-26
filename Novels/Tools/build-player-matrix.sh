#!/bin/zsh
set -euo pipefail

script_dir=${0:A:h}
project_root=${script_dir:h}
somegame_root=${project_root:h}
remote_url=${NOVELS_REMOTE_CONTENT_URL:-https://pureshechka.com/dev}
version=${NOVELS_PLAYER_VERSION:-$(date -u +%Y.%m.%d)}
build_number=${NOVELS_PLAYER_BUILD_NUMBER:-$(( ($(date -u +%s) - 1577836800) / 60 ))}
git_revision=$(git -C "${somegame_root}" rev-parse --short=8 HEAD)
artifact_root=${project_root}/Build/Players/${version}/${build_number}

export NOVELS_PLAYER_VERSION=${version}
export NOVELS_PLAYER_BUILD_NUMBER=${build_number}

for target in Android iOS Windows macOS; do
  case ${target} in
    Android) content_platform=android ; remote_output=Novels.apk ; embedded_output=Novels.apk ;;
    iOS) content_platform=ios ; remote_output=Novels ; embedded_output=Novels ;;
    Windows) content_platform=windows ; remote_output=Novels.exe ; embedded_output=Novels.exe ;;
    macOS) content_platform=editor ; remote_output=Novels.app ; embedded_output=Novels.app ;;
  esac

  "${somegame_root}/Tools/novels-tools/novels-content" build all "${content_platform}"
  "${script_dir}/build-player.sh" Remote "${target}" \
    "${artifact_root}/${target}/Remote/${remote_output}" "${remote_url}"
  "${script_dir}/build-player.sh" Embedded "${target}" \
    "${artifact_root}/${target}/Embedded/${embedded_output}"
done

metadata=${artifact_root}/build-info.txt
mkdir -p "${metadata:h}"
{
  print "version=${version}"
  print "build=${build_number}"
  print "git=${git_revision}"
  print "created_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  print "remote_url=${remote_url}"
} > "${metadata}"

print "Player matrix completed: ${artifact_root}"
