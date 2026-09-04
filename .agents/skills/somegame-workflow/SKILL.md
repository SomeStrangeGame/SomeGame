---
name: somegame-workflow
description: Work safely and efficiently in the SomeGame Unity repository by routing tasks through its canonical context, coordination, implementation, and validation workflows. Use for analysis, edits, Unity work, content, art, documentation, integration, or handoff inside SomeGame; do not use for unrelated repositories.
---

# SomeGame workflow

Treat the Git root containing `Docs/AI/README.md` and `Tools/somegame` as the
only project root. The parent `Fork` directory is not a workspace.

## Start and route the task

1. Read `Docs/AI/README.md` and
   `Docs/AI/rules/ParallelRefactoringCoordination.md` completely before project
   inspection, edits, Unity operations, validation, or builds.
2. Run `Tools/somegame context --task <type>`, choosing the narrowest type:
   `inspect`, `docs`, `code`, `unity`, `content`, `art`, or `integration`.
   Pass exact task-owned `--paths` when known. Use `--resume` in the same chat
   and reread only documents whose returned fingerprint changed.
3. Read every document returned in `documents` completely. Treat those files
   and the current source/configuration as authoritative; do not copy their
   detailed rules into prompts or invent a parallel workflow.
4. Inspect the full `git status --short`. Preserve unfamiliar changes and keep
   the working scope disjoint from active owners.

For a task spanning multiple types, combine the applicable documents while
keeping one minimal exact scope. Use a matching Unity workbench skill in
addition to this skill when the request is specifically Unity onboarding,
implementation, bug investigation, MCP work, project health, or build
validation.

## Choose the execution path

- Read-only analysis requires no lock and must not mutate runtime state.
- Use the documented docs-only fast path only when every listed condition is
  satisfied.
- Before any other write, generator, formatter, Unity operation, test, content
  build, or Player build, create the task's exact agent/request records, wait
  for FIFO, and obtain the matching `active/write-lock/owner.md`.
- Never remove another owner's records, reset unfamiliar changes, broaden scope
  implicitly, or run concurrent Unity/import/build processes.
- Prefer the bounded commands under `Tools/somegame` and
  `Tools/novels-tools` over reconstructing their behavior with ad-hoc shell
  commands.

If the queue, approval, user input, or an external resource blocks progress,
leave a truthful resumable status and do not retain the write lock while
waiting.

## Validate economically

Use current changed-path evidence rather than a fixed broad test matrix:

1. Inspect `Tools/somegame verify --explain` or the task-specific plan.
2. Run the cheapest relevant static checks first.
3. Run only the required content, Editor, test, Player, or manual gates, with
   exact project/platform parameters. Do not launch Unity for docs or tooling
   work unless the plan requires it.
4. Report compact success evidence. Read or quote full logs only when a failure
   needs diagnosis.

Before releasing a normal write lock, perform scoped diff review, update the
shared handoff and own agent status, then remove only the task's own request and
lock. Do not create commits or publish changes unless the user requested that
operation.
