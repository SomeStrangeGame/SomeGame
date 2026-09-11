# Pre-Unity handoff — 2026-09-11

Story `znak-na-dube`, branch `codex/story-znak-na-dube-release`. This adoption
pass applies the current `somegame-create-story` archive and preview contract to
the already-produced candidate; it does not regenerate accepted art or silently
reconstruct missing history.

## Completed outside Unity

- Mandatory brief facts remain explicit: author wording `Тёмное фэнтези`,
  fictional basis, 14+ audience, one 25–45 minute episode, five characters,
  eight decisions, three endings and auto-approve for reversible revisions.
- Complete scenario and canonical Ink remain byte-identical to the prior passed
  narrative/full-text gate; no new prose or choice topology was introduced.
- Scene-derived art, character, audio, Bubble and episode-cover mappings remain
  complete. The one episode maps to its one distinct cover.
- Website preview schema/paths pass and its 17 blocks exactly reproduce the
  canonical opening before the first choice.
- Desktop source review covered the story and episode covers, character sheets,
  location sheets, choice sheet and Bubble kit. Runtime-dependent checks are
  not claimed.
- Archive v005 replaced temporary absolute asset pointers with portable,
  hash-verified story-local references, retained all three episode-cover
  candidates as archived bytes, and added explicit `brief` and `research`
  categories. Earlier manifests remain unchanged.

## Evidence and limits

`check_story.py` passes 372 complete routes, all three endings, 18 options and
zero unreachable executable lines. Seven regression tests pass. The archive
verifier checks predecessor hashes, unique stable artifact IDs, every retained
file and every portable pointer.

Pre-adoption intermediate drafts, original approval timestamps and some exact
generation model/version metadata remain unavailable and are named gaps. Unity
Ink compilation/import, content build, save/load, catalog fallback/crop, Bubble
state matrix, runtime alpha/composition, Player and device replay are outside
this pass. Status is `ready-for-final-validation`, not acceptance or release.

Predecessor manifest: `archive/2026-09-11_manifest_v005.json`, SHA-256
`97af3725657abc218de55505922fff673f6bdeb278e66d03f529cfca30774939`.
The archive tool emits the next exact current manifest after snapshotting this
handoff; the orchestrator reports that path and hash without self-hashing it.
