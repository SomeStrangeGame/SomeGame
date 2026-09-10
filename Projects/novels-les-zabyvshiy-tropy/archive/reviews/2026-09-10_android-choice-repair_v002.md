# Android choice repair — 2026-09-10 — v002

Story: `les-zabyvshiy-tropy`

Candidate APK SHA-256: `96660cf4a935b6c71268700fa39102be5706a8f31f23598181bad9daee61df25`

The v001 blocker was reproduced and traced to a stale composed story bundle whose choice images did not load as Sprites. After correcting the twelve story-local choice importers, explicitly composing repaired release `aa5840ca2751a28b804424b8f62a49c6cd1d540c8db3cd34cfbe20729bec0bcc`, and rebuilding the Embedded player, run `e58999045d79491d939906204a960943` rendered three distinct illustrated cards at the first `s01e01` choice (`choiceCount=3`).

Route A selected choice ID `0`. A process stop/relaunch produced run `92480ae3640f467f8653f2b270d0067c`; the catalog offered `Продолжить` and restored the branch-specific post-choice line «Его зовут Яр…».

Result: the v001 illustrated-choice blocker is repaired and save/resume is verified. Overall story acceptance remains in progress until all episodes, all twelve alternatives and all three endings are covered by the route matrix.

Evidence:

- `Art/Acceptance/20260910-choice-fix/choice-icons-visible.png`
- `Art/Acceptance/20260910-choice-fix/save-resume-catalog.png`
- `Art/Acceptance/20260910-choice-fix/save-resume-restored.png`
- `Art/Acceptance/20260910-choice-fix/runtime-logcat.txt`
