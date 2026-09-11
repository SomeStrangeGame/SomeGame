# Acceptance validation — 2026-09-11

Status: `ready-with-limitations`.

The story candidate `5ea5175d9bf42f86a0a6d4ded6a7555cd9b1a74c` was merged into
local `main`; catalog registration was committed as `154346cf`. The catalog
contains `first-snow` exactly once, as the eighth and final entry.

## Passed

- Atomic story Unity editor build from integrated `main`: passed in 35.038 s.
- Catalog Unity editor build with `first-snow`: passed in 7.017 s.
- Generated catalog registry contains eight unique stories and resolves the
  `first-snow` card, one episode preview and `s01e01` episode cover.
- The built story release contains the canonical Ink JSON, source map and all
  three audited delivery chunks.
- Fresh Novels Editor compile: passed in 7.151 s with zero compiler errors.
- No `INITIALIZATION_FAILED` or Ink compiler timeout was found in fresh logs.
- Generated build output is ignored; the only intended post-merge tracked
  catalog change was `Projects/novels-catalog/Config/catalog.json`.

## Limitations and waiver

The user explicitly waived emulator and ADB checks for this chat. No Android
APK/device/emulator acceptance evidence was produced, and the waiver must not be
reported as a passed device gate.

Manual visual acceptance through the real Catalog-to-story Player flow was not
available in this environment. Editor compilation and content builds do not
substitute for that observation. The five presentation insert images are
bundled but have no authored runtime display commands/prefabs and therefore are
not claimed as visible.

Non-blocking fresh-log diagnostics were limited to the known initial licensing
access-token message, certificate/Xcode discovery diagnostics and shared Ink
integration obsolete-API warnings. They did not prevent successful builds or
compilation.

## Publication

Published to the `kostroma-dev` content channel on 2026-09-11 as immutable
version `first-snow/1`. The existing public APK was retained unchanged.

- Final Android content build and validation: passed.
- The channel manifest retained the six existing entries at their then-current
  versions and appended `first-snow: 1`, for seven stories total.
- The staged and server story trees matched under an `rsync --checksum`
  comparison; all 16 files were present.
- The local and public channel-manifest SHA-256 is
  `52e0581c193d8b5439c0144c1ff6ffc8f73674a76c72048453d5da501bff17c0`.
- Public HTTP checks returned `200` for the card, cover, Android and Mac release
  manifests, episode cover, and a representative Android bundle.
- The live website exposed `Первый снег` in navigation and reported `1 / 7`.
  The access gate prevented an unauthenticated click-through visual review.
- Server access reused the dedicated `~/.ssh/sweb_novels` identity. No key
  contents were read or recorded.

## Publication correction — version 2

The initial public version did not expose the intended beta stage or the
story-owned website preview. Root causes were a missing `releaseStage` field in
the First Snow card and the compose command not copying `Config/Preview` into
the releasable story tree.

Published immutable `first-snow/2` on 2026-09-11. The corrected card contains
`releaseStage: beta`; `preview/preview.json` and its two character images are
present. The seven-entry `kostroma-dev` manifest now selects version `2` and has
SHA-256 `25657e8bae53cdd605371e27b85a679a44d1d2df0b1439285c0b6a8d5d481625`.
The staged and server trees matched under checksum comparison. Public HTTP
checks returned `200` for all three preview files. Live browser inspection
confirmed the `БЕТА` badge, the `Читать начало` button, and an opened preview
ending with `Продолжение — в приложении`.
