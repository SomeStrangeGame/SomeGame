---
name: somegame-accept-story
description: Perform final acceptance of a completed SomeGame story by auditing originality evidence, narrative reachability, assets and selectors, catalog registration, content builds, mandatory Android emulator evidence, manual visual gates, and integration readiness. Use after story project, art, and playable content exist; do not use to create missing production work.
---

# Accept a SomeGame story

Use this skill together with `$somegame-workflow` and read the
[acceptance checklist](references/acceptance-checklist.md). Treat the approved
narrative, project, character, remaining art, and content handoffs as inputs.
This is an evidence gate: do not rewrite the story or manufacture missing
assets to obtain success.

## Establish the acceptance scope

Identify the exact story project, `storyId`, branch/base, expected catalog
position, target platform, approval mode, and already completed manual gates.
Inspect the full dirty tree and active ownership. Separate automated evidence,
manual visual evidence, and deferred platform checks; none substitutes for
another.

## Audit the complete story

Confirm that the approved scenes, meaningful choices, consequences, state
transitions, reconvergence, and endings are reachable and coherent. For factual
stories, verify that the supplied evidence classifications and material
reconstructions survived prose and visuals without overstating certainty.

Audit the complete chain:

```text
narrative scene -> source Ink -> compiled story/source map
                -> selector -> runtime asset -> bundle/release
```

Check card and episode metadata, required backgrounds/media, character
identity and outfits, emotions/poses at their narrative moments, alpha/scale/
gaze/composition, and absence of accidental speculative assets. A fallback or
missing required address is a failure even if the episode continues.

## Audit originality evidence

Fail closed unless the handoffs contain explicit `passed` originality results
for the narrative package, every character package, the non-character art
package, and the complete source Ink. Each result must identify reviewed
iterations, direct sources or declared search limitations, material findings,
targeted revisions, final risk/confidence, and provenance for licensed,
public-domain, adapted, or homage material.

Do not repeat the same searches merely to recreate evidence and do not upgrade a
missing, incomplete, or `blocked` gate during acceptance. A blocked production
gate blocks the story. If a later change materially altered already reviewed
prose or visual work, return that artifact to its owning production skill for a
fresh originality review before acceptance continues.

## Register and validate

Add the story to the catalog only when its card and minimum playable content
exist and the encompassing request authorizes end-to-end story creation.
Preserve the requested catalog position and every existing entry. Catalog-only
mutation does not authorize publication.

Use the current changed-path plan: cheapest static checks first, then story and
catalog validation/build, the mandatory Android emulator gate below, and the
manual visual review required by changed UI or art. Unity Editor may be used as
a technical build/compile mechanism, but interactive Editor replay is not story
acceptance evidence. Distinguish compilation, content build, emulator runtime,
reachability, and visual correctness. Do not broaden to unrelated stories or
platforms.

## Run the Android emulator acceptance gate

Build a fresh Android Embedded APK from the final accepted content and catalog
state. Record the APK path, build timestamp, cryptographic hash, package name,
story release identity, emulator/device model, Android API level, and exact ADB
serial. Install that APK into that emulator and launch the story through the
real catalog-to-story flow; a direct scene launch, Editor Play Mode, or evidence
from an older APK does not satisfy this gate.

Plan the smallest replay/checkpoint set that covers every episode, every
semantically distinct choice branch, and every reachable ending. During those
runs verify ordered smoke/runtime markers, absence of crashes and ANRs,
unexpected errors and `fallback.used`, correct selector and asset resolution,
transitions, save/resume state, and the key visual scenes. Preserve the tested
paths, choices, endings, relevant logs, and screenshots or equivalent visual
observations as acceptance evidence.

Evidence becomes stale when source Ink, compiled content, required assets,
selectors, catalog registration, or the APK changes after the recorded build.
Missing, incomplete, or stale emulator evidence is `blocked`, never
`ready-with-limitations`. Acceptance does not repair a defect found here: return
the affected artifact to its owning skill, rebuild, and rerun the gate.

## Decide and hand off

Return exactly one status:

- `ready-for-integration`: every required automated and manual gate passed;
- `ready-with-limitations`: the story is coherent and validated, but explicitly
  named non-blocking platform/manual evidence remains;
- `blocked`: missing content, fallback, broken reachability, factual conflict,
  missing/failed originality evidence, missing/stale Android emulator evidence,
  validation failure, or another blocking acceptance condition remains.

Report exact project/catalog paths, APK/build/emulator identity, release
evidence, tested paths and endings, runtime logs and markers, manual visual
coverage, warnings, assumptions, Git delta, and the next bounded action. Do not
merge, commit, publish, delete a branch, or downgrade a failed gate without
explicit authority.
