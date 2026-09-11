# Content handoff

- Status: `pre-unity-ready` (current narrative/full-Ink originality and static media preparation passed; Unity-only gates remain).
- Root Ink: `Assets/Ink/nevesta-izo-lda.ink`; episode source: `Assets/Ink/s01e01.ink`.
- Planned compiled outputs: `Assets/Ink/nevesta-izo-lda.ink.json` and `.source-map.json`, deferred to authorized Unity-backed compilation.
- Structure: one episode; five named meaningful choices; three explicit ending knots; state variables are declared at root scope and survive reconvergence. Three mandatory investigation knots were added so every route carries the dramatic evidence needed for the final choice.
- Save compatibility: first unreleased version; stable knot and choice IDs established now and must not be renamed after release.
- Selector audit: all 45 referenced production media files exist. Three definition-local aliases map the runtime protagonist ID `maincharacter` to the existing `Мира` whole sprites; other characters retain name-based paths. This is static address resolution, not proof of Unity import or Player rendering.
- Duration contract: measure reachable route text after compilation; acceptance target is 3,000–5,000 Russian words per complete route (about 25–45 minutes with choice pauses), not aggregate Ink word count.
- Reproducible pre-compile source audit: run `python3 Projects/novels-nevesta-izo-lda/Design/audit_story.py` from this worktree root. All 38 selectable routes pass: `thaw` 6 routes / 3,470–3,529 words; `glass` 16 / 3,372–3,464; `lantern` 16 / 3,429–3,521. Counts exclude selectors, command metadata, choice labels and the end marker. This restricted-source interpreter is not an Ink compiler; compiled route measurement remains required.
- Audited episode SHA-256: `4ec6345555b01988af7f716567693648443457d1ef13171d883757bfb3a75861`. Full issue/change ledger: `Design/STORY_REVIEW.md`.
- Current autonomy/deadline revision supersedes the earlier passes below: Mira's own refusal is effective on both paths; `truth_osip` affects accountability, not the final menu. All final decisions go through `dawn_seal` using `promise_shared` and new `memory_offered` flags. Shared promise appears only when `trust_leda >= 2 && ribbon_free`; there is no failed-shared ending. Six test methods cover all 16 pre-final menus and 38 complete routes, including three ledger-first `thaw` routes. Existing knot/choice IDs are preserved and the shared settlement knot is added; pre-release saves are not claimed compatible with this graph revision.
- Second consistency pass: unified dawn enforcement, separated measurement/gate chains, honored advance emergency consent, supplied village information channels, made Leda/Osip final assent match state, restored the bell and supplied the thaw exit. Four regression test methods now include narrative checks against each route's emitted source lines, not only numerical reachability.
- Third consistency pass: repaired Savva's answer to the revised childhood-letter question and his explanation of Mira's memory loss; removed Mira's unexplained reference to a promise before reading her letter; aligned weir actions with assigned roles and clarified the final house reference. Five test methods now cover adjacent question/answer and instruction/action pairs on all applicable routes. No state, choice, ending condition or world rule changed.
- Static review: balanced knot destinations and all ending routes reviewed in source; objective compile/build validation remains deferred.

## Static pre-Unity audit — 2026-09-07

- `novels-content doctor` passed without launching Unity.
- Card and package JSON parse successfully; all ten relative package dependencies resolve; the root Ink GUID matches `_authoringRootInkGuid`; no duplicate local `.meta` GUID was found.
- All source diverts target one of the 17 defined knots or `END`. Source-driven evaluation reaches 6 `ending_thaw`, 16 `ending_glass`, and 16 `ending_lantern` routes. Every route has exactly one episode end marker, five choices and the common settlement beat; all condition expressions and the final choice guard exercise both outcomes. Correct trust ranges remain Leda 0–3 and village -1–3. All 16 pre-final menus are checked against a contract independent of the source guard; adding an Osip veto or exposing an ineligible promise is rejected. Conditional-choice order was checked against the local Ink parser source; actual Ink compilation is still deferred.
- Selector inventory is exact: 12/12 location PNGs, 10/10 audio files, 5/5 choice PNGs and 18/18 whole-character variants are referenced; no required selector is missing and no production file in those roots is unused.
- Previous container audit: 47 PNG files passed signature, chunk CRC and IDAT decompression checks; all 18 character files are 1024x1536 PNGs declaring an alpha channel. Channel presence does not prove a transparent cutout or correct edges. All ten WAV files are valid stereo 16-bit 48 kHz containers with non-zero frames; this does not prove adequate audio quality.
- Previous source-art inspection is not full visual acceptance: character-background separation, contact-sheet coverage and dedicated light/dark proofs remain unresolved. This narrative revision did not modify or reapprove raster/audio assets; runtime framing/contrast also remains deferred.
- No `Config/build.json`, `.DS_Store`, `Library`, `Temp`, `Logs` or `obj` artifact exists in the story project. Template `.gitignore`, package manifests and Unity version remain byte-identical to the registered template base.
- Pre-Unity media blockers resolved on 2026-09-11: all ten low-range WAV drafts were archived and re-rendered; Mira's standalone contact sheet and dedicated light/dark proofs for all six characters were added. Unity import/mix and runtime edge review remain deferred, as intended.

## Current-skill adoption — 2026-09-11

- Opening orientation now establishes Mira's hydrology/ferry responsibility, immediate crossing stakes, and value conflict before the supernatural incident.
- Evidence notifications occur only after the player has observed the supporting fact and remain branch/ending-local.
- A canonical public excerpt manifest and its two used character images live under `Config/Preview/`; the excerpt stops before the first choice and does not invent text beyond the source.
- Current audit: 7 tests; 38 routes; 16 pre-final menus; 45/45 selectors; route ranges 3,471–3,628 words.
- Immutable before/after narrative snapshots, adopted provenance, decision and review records are indexed by `archive/2026-09-11_manifest_v001.json`.

## Episode catalog cover preparation — 2026-09-08

- Current episode inventory: one episode, `s01e01`; its definition now contains `_catalogCover: s01e01.png`, resolving to `Config/EpisodeCovers/s01e01.png` under the current-main SDK contract. Existing story cover, episode ID, Ink text, aliases and GUIDs are preserved.
- New portrait PNG: 1024×1536 RGB, SHA-256 `591958954c28fdfcebb3bb3271fc27e2eb0502ac8af431ba45bae05af0a9540c`. Source visual/originality review passed for this full new candidate; see `Art/ORIGINALITY_EVIDENCE.md`. This is a file/source binding, not an imported or exported catalog preview.
- SDK history: episode-cover support was originally staged against an older base; the registered worktree was subsequently refreshed to `3e449934bf86ab84ed08853d34845b30f7eacf25`. Reconfirm import behavior at the Unity gate and preserve `_catalogCover`.
- No Unity, catalog mutation, preview export, build, emulator, commit or publication in this cover task. Existing audio and character-evidence blockers remain; the extra Config image is outside the 45 Ink media selectors.
- Static checks passed: PNG signature/all chunk CRCs/IDAT decompression and RGB scanline size; copied-file hash equality; exactly one matching episode/cover binding; six story regression tests and the unchanged 38-route/16-menu/45-selector audit. Scoped text whitespace check passed. These checks do not run the SDK validator or establish Player display.

## Full-text originality gate

- Current gate: `passed`, adoption recheck 2026-09-11; risk `low`, confidence `medium`. The complete current episode SHA-256 `4ec6345555b01988af7f716567693648443457d1ef13171d883757bfb3a75861` is covered by [ORIGINALITY_REVIEW.md](ORIGINALITY_REVIEW.md). Five directly opened comparison sources, the adoption-delta searches and limitations are recorded there. This does not replace Unity/runtime acceptance.

### Historical iteration 5 — not current acceptance evidence

- Iteration 5 (2026-09-07) reviewed the earlier complete source SHA-256 `8bc236e84a991774f6b2882a8d7b7ec2cd3b7f670586bf35e50ce20c4394c06d`: dialogue, narration, transitions, recurring phrases, all conditional text and endings. It superseded iteration 4 for that earlier candidate only.
- Generic motifs excluded: cracking ice, winter bells, forbidden rescue, village secret, dawn ultimatum.
- Distinctive expression reviewed: warmth bubbles as plural memories; a childhood letter preserving refusal without restoring feeling; a drowned orchard storing socially edited recollections; hydrological readings keyed to answers supplied for other people; a collective sluice test in which command destabilizes water and explicit assent calms it; “no one may inherit another's yes.”
- Live searches used compact title/cast combinations, the short inheritance-of-consent phrase and lake/memory/consent/keeper descriptors; no complete unpublished source was uploaded. Search hits alone were not treated as evidence. Direct comparison used the complete [Yuki-Onna text](https://en.wikisource.org/wiki/Yuki-onna): winter/ferry/promise are generic overlap; its secret-identity marriage and forbidden disclosure sequence is absent here. The revised Russian dialogue, hydrological experiment, letters, signature withdrawal and three-way resolution have no substantive matching expression or sequence in that comparison.
- Limitations: no supplied comprehensive corpus; noisy search results; lake/keeper candidate pages on fanfics.me and Litnet failed to open. Their snippets were excluded, so those works are not claimed as fully checked. The review is not proof of absolute uniqueness.
- Iteration 4 additionally searched the lake/contract/memory/consent/bride combination and the compact new hope-versus-consent phrase. No new verified expression match was established; broad result snippets are not comparison evidence. Recomparison of the complete current source with the directly read Hearn text found no substantive dialogue or sequence overlap beyond the already noted generic motifs.
- Iteration 5 used the existing direct-text comparison and a further compact lake/memory/night/Leda search. The returned incidental name/motif hits do not establish expression similarity and were not treated as fully reviewed works. The complete current candidate introduces no substantive overlap with the directly read comparison text; search/corpus limitations remain unchanged. Narrative configuration and ending graph were not redesigned in this pass.
- Final risk `low`, confidence `medium`; provenance original Russian prose authored for this package; result `passed`.

## Deferred gates

After the audio/character-evidence blockers are resolved, Ink compile/source map, Unity import, atomic validation/build, MCP live/restart proof, catalog registration, Android Embedded build and emulator branch replay, and Player visual/Bubble review require the separately authorized final slot. Narrative/full-text originality is current for the hashes in `ORIGINALITY_REVIEW.md`; a material source/graph change requires renewed review.
