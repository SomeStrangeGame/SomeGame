# Episode catalog covers — 2026-09-08

Status: six approved source illustrations mapped for current-main integration; catalog export and Player proof pending. Requested by the author for all six episodes. This requirement is story-local; the SDK still permits an omitted episode cover.

## Frozen scope and art direction

Six distinct portrait PNG illustrations, target 1024×1536 (2:3), matching the existing story cover's geometry. Painterly gouache/charcoal, deep spruce greens and indigo, restrained amber light; dawn is lighter, not a different art style. No baked-in text, logo, border, people, faces or invented character variants. Central readable focal points and quiet peripheral detail support catalog cropping; actual Player framing is still a deferred gate.

| Stable episode ID | Narrative source and cover subject | Target filename | Status |
| --- | --- | --- | --- |
| s01e01 | Station at dusk: copper bell on the gate hook, wet spruce and fading cutline | s01e01.png | approved / mapped |
| s01e02 | Blackwater: wooden bridge and its impossible overhead inverted counterpart | s01e02.png | approved / mapped |
| s01e03 | Doorless cabin: lit square window, continuous log wall without a door, stones visible among trees | s01e03.png | approved / mapped |
| s01e04 | Voice ring: circular spruce grove, bog light shaped like an open doorway beyond it | s01e04.png | approved / mapped |
| s01e05 | Glade: pale trunks, living root arch and an unfinished resin-red line | s01e05.png | approved / mapped |
| s01e06 | All endings: ranger-station threshold after dawn, copper bell and damp forest beyond; no branch-exclusive fate | s01e06.png | approved / mapped |

Approved files live in `Config/EpisodeCovers/`; each `_episodes` entry in `Assets/les-zabyvshiy-tropy.asset` binds `_catalogCover` by filename. Existing `Config/cover.png`, story art and source Ink remain unchanged. No generated preview, bundle or Unity metadata was authored manually. Approval is the agent's source-art review under the existing auto-approve brief, not a claim of user or Player acceptance.

## Completed evidence

- Built-in image generator, six independent calls; pilot s01e01 reviewed before the remaining five. Full submitted prompts and original retained output paths: `EPISODE_COVER_PROMPTS.md`. Selected outputs were copied byte-for-byte; no crop, recolor, raster editing or format conversion.
- All six full images were visually inspected: coherent painted texture, spruce/indigo palette progressing to dawn, no added people/text/logos, recognizable different episode subjects. Doorless wall, inverted bridge, resin line and branch-neutral epilogue checked. These are thematic catalog illustrations, not screenshots or claims that all pictured motifs share one literal camera position in Ink.
- All six PNGs passed Pillow `verify()` and full `load()` using the bundled Python runtime: RGB, 1024×1536. Combined source payload: 18,452,350 bytes; initial catalog network/decode cost remains to be measured. No arbitrary compression policy introduced.
- `check_source.py` checks all six exact episode-ID bindings, file existence, PNG headers/dimensions, unique hashes and no copy of the main cover. In-memory regression probes rejected missing binding, another episode's filename and `../cover.png`; real files were not altered by probes.
- The 72-route story audit and configuration doctor pass. Ink digest remains `5705dd152565c3cb66cc498a87ee249468c6ffcd5887459ee6388bb215e6ed3a`; choices, prose and ending distribution are unchanged. No Unity import/build/Player claim.

## Cover-package originality review — iteration 1

Scope: the complete newly requested six-cover candidate, not individual changed fragments. Earlier background/choice/character art is unchanged and retains its separately recorded limitations; this review does not upgrade that earlier evidence.

Sources checked on 2026-09-08: [Polygon Treehouse official Röki art gallery](https://www.polygon-treehouse.com/roki), including direct browser visual inspection of `Röki_Preview_Clearing.jpg` and `Röki_Announce_Art_Unframed_8.jpg`; [Campo Santo Firewatch](https://www.firewatchgame.com/) was read for descriptive forest/lookout context, not treated as a screenshot comparison. Search used compact public genre descriptions, not uploaded candidate artwork. One additional Röki image fetch failed and was not counted as inspected.

Shared conventions: dark forest depth, cool night colors, woodland architecture and isolated warm light. The inspected Röki art uses broad flat geometric silhouettes, a creature/child clearing or snow-covered village; these covers instead use textured painterly moss/wood, object-led portrait staging, paired inverted bridges, blank-faced stone posts, an intangible doorway and a resin/root knot. No substantial matching composition or distinctive visual combination was identified in this limited comparison. No external artwork was supplied to the image generator, copied into outputs or requested as an artist/IP style.

Result: `passed`, risk low, confidence medium within the bounded two-image visual corpus and descriptive checks. No reverse-image matching service or exhaustive visual index was used; this is not proof of absolute uniqueness. No originality-driven redesign was needed, so the loop ends at iteration 1.

## Deferred gates

- The story worktree still predates episode-cover SDK support. Current-main integration remains a prerequisite for actual export/display; assigning serialized fields alone does not prove runtime support in the old checkout.
- In the separately authorized final slot, build/export all six covers with the platform preview; verify each real episode card, title/controls overlay, portrait/tablet crop and lightweight pre-download availability. All six episodes intentionally have their own cover; the unchanged story cover remains the story-level/failure fallback.
