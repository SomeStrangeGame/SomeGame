# Story-local Bubble presentation

Status: **partial Android visual evidence — full acceptance blocked, 2026-09-09**.
Story acceptance remains blocked until a fresh Android gate, not waived here.
Fresh APK V2 now shows the new panels and both readable/clickable choices;
catalog resume and the keep-reel branch pass. Full presentation matrix was
stopped on an independent Lada quality/alpha defect. Exact artifact and evidence:
`Art/ANDROID_ACCEPTANCE_V2.md`. No full visual or story acceptance is claimed.

Prefab: `Assets/Presentation/bubble/screen-variant.prefab`. The previous claim
that the surfaces were already suitable for this story was incorrect. Android
showed child-derived blue crystal artwork and two blank choice cards. Inspection
confirmed the local PNGs still contain crystals/stars and `_hideChoiceText: 1`.
User explicitly rejected that style; it must not be accepted or rebuilt as final.

Source-only correction: choices are now visible, vertical text-led buttons,
380×88 logical units, centered 20-unit labels with 20-unit horizontal padding.
Dialogue typography is 22 units with warm ivory text; the existing viewport moves
from y=-50 to210 to reserve room for long text and three ending options. All181
serialized objects and1202 fileID/GUID references are unchanged. No runtime,
Ink, character art or shared fallback changes. Static font/layout preflight
passes; actual Unity fit and the full fresh Player matrix below remain pending.

New graphite/aged-bronze artwork is now mapped to both existing runtime PNG paths.
Originality iteration1 resumed after HTTP429 and passed with low risk/medium
confidence; exact sources, comparisons and limits are in `Art/presentation-v2/README.md`.
The large-transparent-margin choice draft was rejected for geometry; the approved
dialogue surface is reused for the button. The two files share SHA256
fc2e8e17bf0226d9b6fffc8c7f2209e80c0fa3c22638da4e6bc95d1c751364cc.
Old child PNGs are replaced only in this clone and recoverable from Git HEAD.
Sprite filenames, GUIDs and all metadata/import/compression settings are intact.
Unity import, final border fitting and bright/dark alpha-edge review are deferred.

Required Player state matrix (not yet validated): long narrator text on `bg10-square-dawn` and `bg09-zero-room`; named Lada text with authored sprite; both two-choice and three-choice final states with the current Ink labels; permitted missing icon fallback; portrait safe area; thirteenth-bell special state represented by restrained static copper emphasis. No flashing motion was introduced.

`Art/presentation-v2/audit.py --self-test` checks286 actual dialogue strings,
63 reachable pre-choice contexts and all13 labels with the shipped font. Worst
conservative body bottom742/1024 and choice bottom967/1024 leave57 units below;
max label58 fits the68-unit text rectangle. The initial96-unit button failed
the48-unit reserve (choice bottom991), so it was reduced to88, not the text.
Both PNGs are genuine RGBA; reading-region alpha252..254 and worst white-background
contrast8.11:1. This is a source estimate, not Unity rendering or face detection.
Five negative UI fixtures detect hidden labels, horizontal cards, old dimensions,
old viewport and broken sprite GUID. Serialization/reference parity and diff pass.
Source Ink
remains unchanged:86 routes, all13 options/three endings and seven rejected
negative fixtures pass. The previous APK is stale for this revised prefab.
New import/build/contrast/wrapping/tap-area checks require a newly approved final
slot for this complete source candidate. No publication performed.
