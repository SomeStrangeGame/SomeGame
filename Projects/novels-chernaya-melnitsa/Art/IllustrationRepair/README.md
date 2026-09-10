# Scene-matched illustration repair — 2026-09-08

Status: both source illustrations repaired, `ready-for-final-validation`.
The successful workshop edit below resolves the earlier service/crop blocker.
User approved both replacements and explicitly authorized closing Unity after
checking unsaved state to finish the remaining workshop correction.
Owner: somegame-produce-story-art + built-in imagegen. No character, Ink,
runtime, catalog, compression/meta, Unity, APK or device changes in this scope.

## Frozen replacement manifest (both source reviews now complete; runtime pending)

| Selector/file under Assets/Locations | Scene | Required composition | Approval |
| --- | --- | --- | --- |
| ill01-mitya-found.png | Ink459–473, narrative scene12, Mitya's chosen later meeting | Unoccupied boat-motor workshop interior in the other settlement, repair bench/motor, two cups for tea, small paper pinwheel outside window; no Chernotalye mill/crossing, baked-in people or fantasy symbols | Author «Да», source review pending |
| ill02-black-flour-hand.png | Ink282–284, grinding-room insert | Clearly readable close view of Lada's open palm with dry black flour, book edge and wet cloth; canon dark olive/brown coat cuff, no added face/body/character sprite | Author «Да», source review pending |

Both are opaque landscape scene rasters, target16:9 (1920x1080 preferred),
painterly cinematic folk-horror, soot/indigo/muted rye-gold; epilogue calmer
daylight without a change of visual medium. Existing whole-character package
is immutable. Hand insert is a self-contained narrative illustration derived
from the supplied whole Lada reference, not a separate generated runtime limb.

Layout: main subjects must survive central portrait cropping. Keep workshop
motor/window/pinwheel in the upper central zone, leaving side foreground for
the existing enlarged character sprites. Keep palm/flour in upper central
third above the narrator Bubble, not under its central/lower text region.
No text, logos, watermark, extra people, gore, glowing runes or unrelated props.

Runtime addresses and existing .meta/GUIDs stay unchanged. Preserve the old
PNGs outside production before replacement. Source checks, full-current-package
non-character originality iteration3 and provenance precede final import.
Older v6 APK evidence remains historical after either PNG changes; fresh
single-story Unity/content/APK/device validation needs separate authorization.

## Final workshop result — 15:50 UTC

Selected `exec-724ad776-19b9-4781-b75c-5a855c101739.png` from the generation
directory below was copied verbatim to `Assets/Locations/ill01-mitya-found.png`.
RGB opaque1672x941, approximately16:9. SHA256850fe43591f213e410786d13db5efdae47df423474fefaec35fa60e59eb69130.
Original .meta/GUID unchanged; both old PNG backups remain intact. The complete
current non-character package passed originality iteration4 with limited
confidence (see ORIGINALITY_EVIDENCE.md). No post-generation image editing.

Motor now centered (head roughlyx44–52%,y23–33%; propellerx45–50%,y51–59%),
pinwheelx57–60%,y20–28%, two cupsx55–63%,y35–39%. Key subjects survive the
approximate centralx37.3–62.7% portrait crop, though the rightmost cup edge is
near that boundary. Unlike the prompt's preferred entire-motor upper-third
placement, the lower unit extends downward on a plausible repair stand. The
recognizable motor head and pinwheel remain above the expected narrator Bubble;
lower motor/propeller and cups may be obscured during dialogue. Overlay fit
remains a fresh Player gate, not a source-only claim. No unrelated mill,
crossing or baked-in character is present; empty lower foreground preserved.

Shutdown: user «Разрешаю», official MCP ready then helper editor_stop; active
scene Assets/Novels/Novels.unity isDirty=false, no compile/reload. Prior Console
error was the known capture_game_view outside Play Mode from15:37. Menu File/Quit
unavailable; soft SIGTERM closed verified Editor98697 then Hub98758, with child
workers absent. No SIGKILL, scene save/discard, new build or ADB. Helper stopped.
No unsaved active-scene change found. Existing catalog/runtime code untouched;
old v6 APK and Mac story bundle are stale for both corrected illustrations.

## Earlier partial result and provenance (historical)

Built-in imagegen only; exact prompts: [PROMPTS.md](PROMPTS.md).
Generation directory:
`/Users/iantonishin/.codex/generated_images/01a07c22-11cb-77c3-906a-b61ce696e845/`.

- Workshop initial `exec-1f0f319b-e7d0-4e80-93e3-16b13e5919d5.png`: narrative
  workshop/tea/window/pinwheel correct, but motor x31–42% is clipped by the
  portrait central crop. Rejected for production. Two identical targeted edit
  requests failed HTTP500, request IDs b6a78629-bf00-44a8-9e21-803ea7dcb5bc and
  e69da386-2c74-4527-a074-36c5a7f37e09. No edited output returned; old ill01 remains.
- Hand initial `exec-2b81e9fd-b211-4c10-9a13-c3dacf4b1601.png`: narrative content
  correct but palm overlaps the planned narrator region; draft only.
- Selected hand `exec-67b7a530-bd28-48ef-89ba-3f80885082dd.png`: palm/flour in
  upper central frame, coherent single attached hand, five digits, canon coat
  cuff, wet cloth and closed book. RGB opaque 1672x941, approximately16:9
  (generator output, not silently resized to the preferred1920x1080). Copied
  verbatim to `Assets/Locations/ill02-black-flour-hand.png` after iteration3.

Initial hand inputs: full immutable Lada runtime reference
`Assets/Characters/maincharacter/view/whole/coat/main.png` for skin/cuff, and
`Assets/Locations/bg11-mill-undercroft.png` for medium/palette only. The targeted
edit used its own initial hand draft. No external comparison image was used as
generation input. No cropping/compositing/pixel editing followed generation.

Source check: [source-check.json](source-check.json). Both .meta files are
byte-identical to backup, preserving GUID/settings. Old twoPNGs/twoMetas remain
in `Novels/Build/Logs/automation/chernaya-illustrations-20260908/before/`.
ill02 selector remains Ink282; ill01 remains Ink459. No characters/Ink/catalog/
runtime source changed in this correction. Fresh current full-package
non-character originality iteration3 passed with explicitly limited confidence;
final workshop change needs iteration4. No runtime visual pass is claimed.

At1080x2400 center-fill, approximatelyx37.3–62.7% remains visible. Selected
palm/powder lies at roughlyx40–59%,y13–30%, above the expected narrator region;
book edge/cloth remain near it. This is source-composition review, not a rendered
Player proof with Bubble/characters. Verify actual frame and transitions after
a fresh approved single-story build. Old v6 APK and ACCEPTANCE_CANDIDATE.json are
historical and do not validate the new PNG; their character-focused hash check
does not establish location-art freshness. R01 completion and R02–R05 remain open.
