# Story review — 2026-09-07

Scope: `Projects/novels-les-zabyvshiy-tropy/` in its registered worktree, based on `403c7692` with the three existing local edits preserved. Read-only shared-source inspection established runtime/Ink contracts; no shared files changed.

## Corrected defects

- The declared protagonist was stored under `ника`, but `CharacterSpriteResolver.Resolve` maps the MainCharacter role to `CharacterAssetProfile.MainCharacterAssetId` (`maincharacter`). Moved the four PNGs to that canonical directory without changing their bytes. Previous name-only selector checks missed this failure.
- The named choice `keep_map` collided with global `VAR keep_map`; the vendored Ink compiler's `Story.CheckForNamingCollisions` forbids that collision. Renamed only the choice label to `preserve_map`; the boolean and icon ID remain unchanged.
- `Голос Ники`, `Голос Аси`, `Голос Фили` were parsed as three absent characters. Their lines now use narrator commands while retaining attributed quoted voices.
- Bridge-rescue callbacks in episodes 4 and 5 now depend on the actual guide choice. Trusting Lada references her guidance; trusting the compass references her rescue.
- Asya's promise and four knots consistently refer to Yar and three missing children. The children return; Yar remains. Removed the conflicting account of three adult rescuers and the earlier claim that Asya had made no promise.
- The late confession now reveals Asya's responsibility for the experiment, consistent with her earlier admission about the resin, instead of accusing her of a claim she never made.
- The archive replays actual remembered events. Asya speaks her previously unsaid warning in the present; the forest no longer invents a past utterance in conflict with its established limits. Filya explicitly remembers his mother's explanation of the three/seven discrepancy.
- The map room establishes its table and stove before either is used. The exit opens after both map choices. Burning now chars the edge, matching the later ash, and preserves the written warning.
- Clarified which map Nika carries, simultaneous crossing, the whistle's transfer, the return of belongings and recognition of the face-down photograph. The line beneath Yar is on the ground, not mysteriously on Nika's possibly burned map.
- Before the final choice, Lada explains whether the current state can sustain binding. A failed bind has an explicit causal transition into the nameless ending. Existing ending predicates remain unchanged.
- The nameless ending no longer invents loss of all dates/addresses or assigns Nika's photograph action to Yar. Lada's name is chosen anew.
- Added the existing music command at episodes 2–5 entry so these episode scopes do not rely on episode 1's audio controller surviving.
- Fixed a bell hanging on an allegedly empty hook and letters appearing on an unmarked palm.
- Corrected documentation: five decisions versus twelve options; actual state names and ending predicate; exact source-route word count. Removed the unverified assertion that runtime provides a fixed 16:9 background crop.

## Verification and limits

`python3 Projects/novels-les-zabyvshiy-tropy/Art/check_source.py` walks the actual authored subset, rejects unsupported syntax, checks selectors including indented branches and protagonist remapping, checks symbol collisions, and explores all 72 routes. Each traverses six episodes and five choice groups, with five episode-end markers and one story-end marker. Endings: shared 24, white-map 9, nameless 39. Route checks reject impossible bridge callbacks, a missing common exit and unexplained failed binding.

This is deliberately not Ink compilation, a save roundtrip, AssetDatabase import, bundle validation or Player proof. Those gates remain deferred. The static script lives outside `Assets` and writes no output files. The former static reports did not establish runtime readiness.

Source changes keep global variable names/types, all episode/ending knots, group counts and ending predicates. The collision fix changes one named choice path, so compatibility with any private experimental Ink snapshot is not promised; use a fresh save for the first validation. The story has never been catalog-registered or compiled according to existing evidence; content version remains 1.

## Remaining visual/evidence checks

Background source dimensions (627×627) do not meet the original 16:9 manifest target; source art was not silently cropped or declared compliant. Actual layout and acceptable framing must be assessed at the visual gate. Character alpha proofs/identity sheets are not present as auditable local artifacts, and dark-matte edges remain unverified. Existing art review limitations are preserved. Duration is a target awaiting measured playback, not a confirmed 27–34 minute result.

No Unity/build, Catalog mutation, runtime coordination mutation, commit, merge or publication was performed in this review.

## Second continuity pass

- Introduced Filya's four-note melody before the bridge choice; the compass route no longer repeats an unintroduced tune.
- Changed the grandfather's quoted claim to a collective rescue, consistent with Filya's later admission that he credited all seven rescues to one person.
- Placed Yar's inscription before the final expedition and clarified earlier short trials, avoiding an unexplained inscription after his disappearance.
- Replaced the physical coincidence of a map hole and a mark on a distant stone with Nika recalling the stone's mark.
- Lada lacks a childhood, not all memory: she remembers Yar asking her to wait. Her silence in the voice ring means absence of her own echo. Her closed jar is pocketed before she must carry other belongings.
- Added an explicit warning that binding may exclude Lada, so that price is known before choosing the white-map ending.
- Completed mutual witnessing in the shared ending: Nika remembers Yar's apple-sharing action, Lada remembers Asya's confession; all five now have another witness. Choices, variables, predicates and ending counts remain unchanged.

The source audit now additionally checks melody/callback order, Lada's warning, the closed jar's carrying arrangement and coverage of all five people in shared-memory dialogue. Current counts/digest are in `READY_FOR_FINAL_VALIDATION.md`; the previous review's visual/Unity limitations still apply.

## Third continuity pass

Reread all six episodes, including all branches and endings, against the narrative package. No additional source-level continuity defect was confirmed. Corrected the package's overly broad promise-until-dawn rule: the travellers' current anchor is distinct from Asya's unfulfilled promise, which explicitly persists as Lada across nights. Added omitted `map_debt` effects to the episode 3–4 scene matrix and labelled the older originality/search limitation as historical.

Ink, choice semantics and artwork are unchanged in this pass; the iteration 4 source digest remains `5705dd152565c3cb66cc498a87ee249468c6ffcd5887459ee6388bb215e6ed3a`. The bounded audit still passes all 72 routes (24 shared, 9 white-map, 39 nameless). These documentation corrections introduce no new story scene, expression or originality comparison candidate. All deferred runtime, duration and visual checks above remain open.
