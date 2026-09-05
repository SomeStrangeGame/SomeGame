---
name: somegame-author-story-content
description: Turn an approved SomeGame story package and art manifest into originality-screened atomic runtime content, including metadata, Ink, selectors, compiled story, and source map. Use for authoring a new story's playable content; do not use for story ideation, art production, or routine fixes to an existing story.
---

# Author SomeGame story content

Use this skill together with `$somegame-workflow`. Start only when the narrative
package is approved, `$somegame-create-unity-project` has produced a statically
valid atomic scaffold with MCP configured for deferred final proof,
`$somegame-create-character` has handed off the approved
character packages, and `$somegame-produce-story-art` has handed off the
remaining approved art manifest and available runtime addresses.

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

After metadata, enter the first real scene directly. Do not add a one-option
`Начать`, `Начать историю`, or `Играть` choice: opening the story already starts
the episode, and the first visible choice must represent meaningful agency.

Connect only approved assets that actually exist. Place location, character,
outfit, emotion, pose, choice, presentation, audio, and video changes at their
narrative moments. Verify each selector against the handed-off runtime address
and current fallback semantics. A missing required asset returns to
`$somegame-create-character` for character appearances or
`$somegame-produce-story-art` for other art; do not substitute or generate art
inside this skill without expanding the task explicitly.

## Pass the full-text originality gate

After the complete source Ink draft exists, apply
`Docs/AI/rules/OriginalityReviewProtocol.md`. Compare distinctive dialogue,
narration, scene transitions, recurring verbal motifs and unusually specific
sequences introduced during prose authoring. Preserve approved choices, state,
factual classifications and narrative intent; return to
`$somegame-design-story` if a safe correction would materially alter them.

## Prepare final compilation inputs

Only after the full-text originality gate passes, complete source Ink and every
input needed to produce compiled story JSON and source map. At this stage run
only dependency-free/static syntax, structure, selector and reachability
checks. Do not launch Unity, compile Ink through a Unity-backed pipeline, import,
content build, compile C#, build Player/APK or use an emulator. Hand the complete
candidate to final acceptance, which enters deferred gates through the
authorization contract in `Docs/AI/rules/UnityConcurrency.md`.

Hand off exact source and planned compiled paths, episode and choice structure,
state and save-compatibility decisions, selector-to-asset audit, static
evidence, text-originality sources, iteration log and final gate result,
warnings, and the deferred compile/build/visual gates. Mark the result
`ready-for-final-validation`. This skill never registers the story in the
catalog; `$somegame-accept-story` owns that mutation. It also does not merge or
publish.
