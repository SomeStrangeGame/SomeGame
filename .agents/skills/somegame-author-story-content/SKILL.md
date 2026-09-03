---
name: somegame-author-story-content
description: Turn an approved SomeGame story package and art manifest into atomic runtime content, including metadata, Ink, selectors, compiled story, and source map. Use for authoring a new story's playable content; do not use for story ideation, art production, or routine fixes to an existing story.
---

# Author SomeGame story content

Use this skill together with `$somegame-workflow`. Start only when the narrative
package is approved, `$somegame-create-unity-project` has produced an MCP-ready
atomic project, and `$somegame-produce-story-art` has handed off the approved
art manifest and available runtime addresses.

## Establish the content contract

Read the current content-authoring, Ink-syntax, pipeline, character, and manual
review documents selected by `Tools/somegame context --task content`. Inspect
the target project rather than copying paths from another story. Preserve its
stable `storyId`, author wording, branch structure, choice semantics, factual
classifications, and approved scene order.

Create or complete the story marker/card, cover reference, definition, episode
metadata, source Ink, and only the supporting configuration required by the
current atomic-project contract. Do not introduce `Config/build.json`, manual
AssetBundle labels, Game dependencies, or story-specific SDK hardcodes.

## Author playable Ink

Translate the approved scene package into reachable knots, stitches, choices,
consequences, endings, and persistent state. Choices must preserve the intended
agency and reconvergence plan; do not add cosmetic branches merely to inflate
choice count. Use canonical Ink command syntax and edit source `.ink`, never
compiled JSON directly.

Connect only approved assets that actually exist. Place location, character,
outfit, emotion, pose, choice, presentation, audio, and video changes at their
narrative moments. Verify each selector against the handed-off runtime address
and current fallback semantics. A missing required asset returns to
`$somegame-produce-story-art`; do not substitute or generate art inside this
skill without expanding the task explicitly.

## Compile and validate

Compile through the canonical SomeGame content pipeline and confirm the source
Ink, compiled story JSON, and source map all changed coherently. Run validation
before build, then only the target-specific gates required by the current
changed-path plan. Inspect warnings rather than treating a process exit alone
as narrative correctness.

Hand off exact source/compiled paths, episode and choice structure, state and
save-compatibility decisions, selector-to-asset audit, validation/build
evidence, warnings, and unresolved visual gates. Do not add the story to the
catalog, merge, or publish unless the encompassing task explicitly includes
that separately authorized step.
