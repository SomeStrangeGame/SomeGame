# Story creation archive

Read this reference when starting, resuming or handing off a story-creation
stage. The creation orchestrator owns the archive contract; stage owners
supply their actual inputs, outputs and review evidence. This is a record of
the creative process, not an originality verdict or a guarantee of authorship.

## Location and lifecycle

Maintain `Projects/novels-<storyId>/archive/` for each story created through
this workflow. Keep it outside Unity's `Assets/`, Resources, runtime content
and public preview directories. The archive is not an alternate source of
playable content and must not be packaged into a build or website by default.

Before the story project exists, start the same archive layout in an authorized
non-Git staging directory, keyed by the stable story ID. This does not authorize
early project creation or bypass the existing pre-production gates. At the
normal project-creation stage, transfer the accumulated archive into the
story's exact owned prefix; verify copied files, hashes and relative links
before discarding any staging copy. A temporary path alone is not a durable
handoff for a completed stage.

Checkpoint as work happens: the initial brief; every saved intermediate draft
or generated candidate; author feedback and resulting revisions; rejected
alternatives; approvals; stage handoffs; and completed checks. Preserve a
candidate before replacing it, not merely a retrospective summary at the end.
This concerns actual creative artifacts and explicit decision summaries, not
every keystroke, hidden reasoning or raw execution trace.

For an existing story, preserve the material that really remains and apply
this process from adoption onward. Record unavailable earlier versions as
gaps. Do not recreate missing drafts, prompts, approvals or dates and present
them as contemporaneous evidence. Populating historical project archives or
renaming production documents requires that task to be in scope; updating
this skill alone does not perform that migration.

## Date every archived document name

Every document saved in the archive, including indexes, manifests, prompts,
feedback, decisions, source snapshots and validation reports, has a date in
its filename. Do not use undated `README.md`, `manifest.json`, `latest.md`
or similar document names inside the archive.

Use `YYYY-MM-DD_<topic>_vNNN.<ext>`, with the snapshot date in UTC. Include an
episode or asset ID when useful. Increase the version for a new snapshot of
the same logical artifact, even on the same day; never overwrite a collision.
When time-level ordering is useful, use
`YYYY-MM-DD_HHMMSSZ_<topic>_vNNN.<ext>` and retain the revision suffix.
Timestamps must come from the actual clock, not an example or inferred history.

Examples of the naming form, not dates to reuse:

- `2026-09-10_brief_v001.md`
- `2026-09-10_s01e02-scenario_v003.md`
- `2026-09-10_scenario-feedback_v002.md`
- `2026-09-10_character-lada-generation_v004.md`
- `2026-09-10_branch-review_v001.md`
- `2026-09-10_manifest_v005.json`

Date the title/header of newly written human-readable archive records too.
Preserve the bytes of copied originals; their dated filename and manifest
entry carry the archival date without rewriting their contents. Record the
original filename, source creation time when known, and actual archival time
separately. An old file imported today receives today's snapshot date; do not
pretend it was archived on its original creation date.

Production names such as `SKILL.md`, `Config/card.json`,
`NARRATIVE_PACKAGE.md` and canonical `.ink` paths remain unchanged. Save
dated archive copies rather than rename files that tools or links depend on.
Use dated names for independent media candidates too; preserve a native
multi-file package's internal names in a dated container with a path mapping.

## What to retain

Create these categories as material exists; do not invent artifacts or create
empty evidence merely to fill folders.

| Directory | Material |
| --- | --- |
| `brief/` | Initial idea, author constraints, relevant user instructions and clarification history. |
| `scenarios/` | Synopses, complete draft versions, episodes, alternative endings, world rules, character arcs, choice/state diagrams and source Ink snapshots. |
| `decisions/` | Author feedback, concise explicit reasons for revisions, selected/rejected alternatives, approval records and stage handoffs. |
| `research/` | Source links and retrieval dates, permitted excerpts, research notes, provenance/licensing information and factual classifications. |
| `assets/` | Actual sketches, character/background variants, audio/video candidates, generation requests and their input/output associations, including rejected versions. |
| `reviews/` | Originality-review outputs, narrative and branch checks, validation reports, screenshots and relevant evidence from tests actually run. |

Preserve the chain `request -> candidate -> feedback/decision -> revision`.
An approved result does not erase rejected attempts. A change summary is useful
alongside a full draft, not instead of retaining the draft itself. Keep a
clear mapping from archived candidates to the chosen production artifact and,
when actually available, its commit or release identity.

Record the supplied author identity without inventing credits. Distinguish
human instructions/edits, AI-assisted or generated material, and third-party
inputs. Retain the relevant generation prompt and tool/model/version when
available; use `unknown` for unavailable metadata rather than guessing.
Save relevant permitted user-facing exchanges, not unrelated conversations,
private reasoning, system instructions or entire session dumps.

## Dated manifest and provenance

Write a new dated `*_manifest_vNNN.json` snapshot at each stage handoff or
meaningful batch of additions. Preserve earlier manifests. Each new manifest
identifies its predecessor when present; the handoff names the exact current
manifest path so readers do not depend on an undated mutable index.

Record the story ID, manifest version and UTC recording time. For each artifact,
include:

- stable artifact ID, category/stage, dated path or dated external-storage
  record, format and SHA-256 of the retained bytes;
- UTC snapshot time, known original/source date or explicit `unknown`,
  original name/location and relevant source reference;
- creator/source role, known tool/model metadata, and links to the request,
  input artifacts, parent revision and explicit feedback/decision;
- candidate/rejected/selected/approved status, reason when recorded, and
  who actually approved what and when; a model review is not human approval;
- production mapping when selected, storage/availability, redactions and
  known gaps.

Verify hashes rather than merely claiming they were computed. Do not include
a manifest's own hash inside itself; record that hash in its handoff or the
next manifest. Hashes and version history help inspect integrity, but local
timestamps or hashes alone do not independently establish who authored a work.

## Storage, privacy and publication

Keep small text records and manifests versionable with the project's existing
Git policy. Preserve large binary candidates using the approved LFS or durable
artifact-storage setup; keep dated pointer records with the location, size,
hash and access/retention limitations in `archive/`. Verify availability
before treating an external pointer as retained evidence. A dead URL or an
expired temporary file is a gap, not a preserved artifact.

Do not configure LFS, upload to a new service, change repository visibility,
commit, push or publish merely because archiving is required. If no approved
large-file storage exists, retain the candidate in authorized local storage
and disclose the retention limitation instead of dropping it silently.
Collecting evidence does not authorize public distribution of it.

Exclude credentials, private tokens, unrelated personal data and confidential
third-party content. Sanitize relevant exchanges before archiving, identify
redactions and hash the retained sanitized bytes. Keep source links and lawful
excerpts rather than copying whole third-party works without permission.
Archive files are not automatically included in website source excerpts,
runtime bundles or releases; review the scope before any authorized publication.

## Handoff and completeness check

Every stage supplies its dated artifact paths, current manifest and any gaps.
The orchestrator reconciles stage outputs into this archive without changing
ownership of art production, originality review or runtime acceptance.

Before handoff, check date naming, revision collisions, referenced files,
hashes, predecessor links, status/approval provenance and input/output mappings.
Check that the actual saved candidates and feedback since archive adoption
are represented, including rejected variants; verify staged material reached
the story archive once the project exists. Do not relabel a missing history
as complete or count an archive as proof that a deferred test ran.

An incomplete required archive returns to the producing stage for correction
or an explicit author decision about an irrecoverable gap. Acceptance records
its own results after running its permitted gates and refreshes the manifest;
archiving never authorizes Unity, device tests or publication.
