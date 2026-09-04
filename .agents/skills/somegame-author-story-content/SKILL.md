---
name: somegame-author-story-content
description: Turn an approved SomeGame story package and art manifest into originality-screened atomic runtime content, including metadata, Ink, selectors, compiled story, and source map. Use for authoring a new story's playable content; do not use for story ideation, art production, or routine fixes to an existing story.
---

# Author SomeGame story content

Use this skill together with `$somegame-workflow`. Start only when the narrative
package is approved, `$somegame-create-unity-project` has produced an MCP-ready
atomic project, `$somegame-create-character` has handed off the approved
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

After the complete source Ink draft exists and before final compilation, run up
to five text-originality reviews. Each review extracts compact fingerprints from
distinctive dialogue, narration, scene transitions, recurring verbal motifs,
and unusually specific sequences introduced during full-prose authoring. Search
available public sources and any author-supplied corpus; inspect direct sources
rather than relying on snippets and do not upload or reproduce the full
unpublished Ink merely to search it.

Record matched expression or arrangement, meaningful differences, direct
source, provenance, confidence, and `low`, `medium`, or `high` risk. Medium and
high findings are material. Common phrases, idioms, genre conventions,
historical facts, necessary command syntax, and short functional UI text do not
establish material similarity by themselves.

Exit immediately with `passed` when no material finding remains. Otherwise,
before review 5, revise only source `.ink` for the next review, changing the
underlying expression or specific scene construction rather than swapping names
or mechanically paraphrasing. Preserve approved choices, state, factual
classifications, and narrative intent; return to `$somegame-design-story` when a
safe revision would materially alter its approved package.

If material risk remains after review 5, mark content authoring and the
downstream story workflow `blocked`. Report direct sources, implicated Ink
locations/elements, risk/confidence, and the five-review change log. Intentional
adaptation, licensed/public-domain material, and homage require explicit
provenance and attribution handling. This gate is not legal clearance or proof
of absolute uniqueness; insufficient source access must be disclosed and may
not be silently treated as a pass.

## Compile and validate

Only after the full-text originality gate passes, compile through the canonical
SomeGame content pipeline and confirm the source Ink, compiled story JSON, and
source map all changed coherently. Run validation before build, then only the
target-specific gates required by the current changed-path plan. Inspect
warnings rather than treating a process exit alone as narrative correctness.

Hand off exact source/compiled paths, episode and choice structure, state and
save-compatibility decisions, selector-to-asset audit, validation/build
evidence, text-originality sources, iteration log and final gate result,
warnings, and unresolved visual gates. Do not add the story to the catalog,
merge, or publish unless the encompassing task explicitly includes that
separately authorized step.
