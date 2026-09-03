---
name: somegame-accept-story
description: Perform final acceptance of a completed SomeGame story by auditing narrative reachability, assets and selectors, catalog registration, content builds, live runtime evidence, manual visual gates, and integration readiness. Use after story project, art, and playable content exist; do not use to create missing production work.
---

# Accept a SomeGame story

Use this skill together with `$somegame-workflow` and read the
[acceptance checklist](references/acceptance-checklist.md). Treat the approved
narrative, project, art, and content handoffs as inputs. This is an evidence
gate: do not rewrite the story or manufacture missing assets to obtain success.

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

## Register and validate

Add the story to the catalog only when its card and minimum playable content
exist and the encompassing request authorizes end-to-end story creation.
Preserve the requested catalog position and every existing entry. Catalog-only
mutation does not authorize publication.

Use the current changed-path plan: cheapest static checks first, then story and
catalog validation/build, one bounded Editor/runtime check, and the manual
visual review required by changed UI or art. Inspect fresh Console/runtime
markers and distinguish compilation, content build, reachability, and visual
correctness. Do not broaden to every story or platform without evidence.

## Decide and hand off

Return exactly one status:

- `ready-for-integration`: every required automated and manual gate passed;
- `ready-with-limitations`: the story is coherent and validated, but explicitly
  named non-blocking platform/manual evidence remains;
- `blocked`: missing content, fallback, broken reachability, factual conflict,
  validation failure, or another blocking acceptance condition remains.

Report exact project/catalog paths, release evidence, tested paths and endings,
manual visual coverage, warnings, assumptions, Git delta, and the next bounded
action. Do not merge, commit, publish, delete a branch, or downgrade a failed
gate without explicit authority.
