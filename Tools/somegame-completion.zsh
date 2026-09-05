#compdef somegame

_somegame() {
  local -a commands
  commands=(
    'context:show scoped project context'
    'start-task:create a FIFO request and acquire the lock when first'
    'queue-status:show FIFO and process state'
    'queue-prune:remove one terminal orphan request'
    'resource-lock:manage shared Unity, catalog, SDK and integration locks'
    'story-worktree:create, inspect or safely remove a registered story worktree'
    'story-candidate:record a clean story commit for integration'
    'story-batch-plan:validate and order story candidates for integration'
    'verify:run changed-path validation'
    'tooling-tests:run all local tooling unit tests'
    'story-check:validate or build one atomic story'
    'content-gate:run a content build gate'
    'editor-gate:run a bounded Unity Editor gate'
    'player-build:build one Player target'
    'android-smoke:install and smoke an existing APK'
    'android-dev-cycle:build, install and smoke an Embedded Android APK'
    'clean-generated:preview or remove generated Unity directories'
    'docs-check:validate AI docs and tooling'
    'commit-plan:group dirty paths for commits'
    'finish-check:inspect finalization blockers'
    'finish-task:validate, hand off and release the current task'
    'git-publish:publish prepared commits safely'
    'licensing-preflight:inspect or recover Unity licensing state'
  )
  _describe 'workflow' commands
}

_somegame "$@"
