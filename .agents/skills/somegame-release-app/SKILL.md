---
name: somegame-release-app
description: Safely publish or update a branded SomeGame Remote Player, its APK when required, and selected versioned stories in a dev or prod content channel. Use when asked to add, deploy, upload, or refresh stories or a standalone app/APK; do not use for store submission, source-code Git publication, or authoring unfinished stories.
---

# Release a SomeGame app

Use this skill together with `$somegame-workflow` and
`$unity-workbench:unity-build-validation`. Current repository documentation,
tool help, app profiles, catalog configuration, and generated manifests are
authoritative.

## Establish the release contract

Before an application has an approved public release brand, give every
technical or prerelease app profile a stable codename that is the name of a
Russian city (for example, `kostroma`). Use that city consistently for the
profile ID, displayed product name, package/application IDs, channel manifests,
build directories, APK filename, and prerelease site. This reduces accidental
collisions with unverified commercial brands. Do not silently promote the city
codename into a store brand: choosing the final public name requires a separate
brand decision and originality/trademark review. A project has no additional
"technical" display name; `dev` remains only the machine channel suffix.

Resolve these inputs before changing runtime state:

- app profile ID from `Projects/apps/<app-id>/Config/player.json`;
- target platform and `Remote` mode;
- intended story additions/version updates and the current ordered channel
  manifest; removal or reordering requires a separately explicit request;
- environment (`dev` or `prod`), derived manifest channel
  `<app-id>-<environment>`, and one shared absolute HTTPS content root, normally
  a dedicated prefix such as `https://example.com/content`;
- signing mode and intended audience;
- exact deployment host, remote paths, and public artifact URLs.

Do not infer a production release, production signing, store submission, remote
host, SSH identity, or permission to remove old releases. A test-signed APK is
appropriate only when the user accepts a test/dev artifact. Treat Git publish
as a separate workflow.

Read the selected app profile, current public channel manifest, story cards, and relevant
sections of `Docs/AI/guides/ContentPipeline.md` and
`Docs/AI/guides/AutomationRunners.md`. Do not edit an unrelated or WIP story
merely to make a broad build pass. A request to publish one story means add or
update it while retaining every existing channel entry; it never implicitly
authorizes replacing the channel with only that story.

## Keep channels in one content root

Use one shared immutable story store for every app and channel:

```text
<content-root>/
  <app-id>-dev.json
  <app-id>-prod.json
  stories/<story-id>/<version>/...
```

The Player receives the common `<content-root>` without an app or environment
suffix. Select its manifest independently with
`NOVELS_CONTENT_CHANNEL=<app-id>-<dev|prod>` when building. The selected
app/environment manifest is the only mutable selector for that release and
contains its exact ordered `story-id -> version` map. For an incremental update,
the desired map is the ordered union of the public map and requested changes:
update an existing key in place and append a new key. Before any write, report
retained, added, updated, removed, and reordered entries. Fail closed if
anything would be removed or reordered without explicit authorization.

Application ownership exists only at the manifest layer. Do not nest the shared
story tree below an app directory: multiple applications may select the same
`stories/<story-id>/<version>` path without copying its bytes. App-specific
catalog UI remains embedded in each Remote Player. In Remote mode, the channel
manifest supplies `StoryIds`; the embedded catalog registry is only a fallback
for modes without that list and does not limit compatible story additions.

## Decide whether the APK changes

Default to a content-only release and reuse the published APK when all of the
following are proven:

- the deployed Remote Player already points to the intended content root,
  app-scoped channel, app profile, and package identity;
- every changed story release uses a schema supported by that Player and its
  `minimumClientVersion` is not newer than the deployed client;
- the story uses no new runtime, catalog UI, delivery, or platform capability;
- only immutable story bytes and the mutable channel manifest change.

Build and publish a new APK only when one condition fails or cannot be proven,
or when the user explicitly requests it. Runtime code, catalog UI, supported
schemas, root/channel selection, branding/profile, permissions, signing,
package identity, and required platform behavior are APK changes. Record the
compatibility evidence either way. Do not rebuild merely because a compatible
story was added to the Remote manifest.

Do not publish duplicate copies under `dev/stories` and `prod/stories` merely
to separate release sets. When both manifests select the same immutable story
version, they must resolve to the same `stories/<story-id>/<version>` tree.
Different channel versions coexist as separate immutable version directories.

Treat per-channel roots such as `<site-root>/dev` and `<site-root>/prod`, and
generic manifests such as `dev.json` and `prod.json`, as a legacy compatibility
layout. When migrating, create and verify the new app-scoped manifests and
shared root alongside the legacy paths. Keep generic manifests only as explicit
temporary aliases for known older Players. Do not alter or remove legacy paths
until replacement Players have passed public runtime checks and the user
separately authorizes their retirement.

## Build content, and the APK only when required

Follow repository FIFO/write-lock and shared `unity` plus `catalog` resource
locks. Obtain fresh human authorization immediately before the release build as
required by `Docs/AI/rules/UnityConcurrency.md`.

Prefer explicit content targets when the repository contains other atomic
projects:

```bash
Tools/novels-tools/novels-content build catalog <platform>
Tools/novels-tools/novels-content build <story-id> <platform>
```

If the compatibility decision requires a Player, build one from those verified
current outputs. Pass the shared content root and select the channel separately:

```bash
NOVELS_CONTENT_CHANNEL=<app-id>-<dev|prod> \
  Tools/somegame player-build --agent-id <agent> --app <app-id> \
  --target <platform> --mode Remote --remote-url <https-content-root> \
  --skip-content-build --human-approved --approval-note <evidence> \
  [--test-signing]
```

Use `--skip-content-build` only when catalog and every selected story were
successfully rebuilt or otherwise proven current in the same release context.
Avoid `build all` unless every discovered atomic project is intentionally part
of the release and production-ready.

Remote Player embeds the current catalog UI under
`StreamingAssets/NovelCatalog`. Therefore the versioned channel layout normally
publishes the channel manifest and story trees, not a separate server-side
`catalog` directory. The embedded registry is not the Remote story selector.

## Stage an immutable channel

Create the channel snapshot only from verified local outputs:

```bash
Tools/novels-tools/novels-content stage-channel <dev|prod> \
  --base-manifest <downloaded-current-manifest.json> \
  <story-id=version> [<story-id=version> ...]
```

The safe default preserves existing order and entries, updates a version in
place, appends new stories, and emits a machine-readable merge summary plus a
temporary `dev.json` or `prod.json`. Inspect it, require exact equality with the
intended ordered union, and publish those verified bytes under the final
`<app-id>-<environment>.json` name. Do not publish or replace a generic manifest
unless the release contract explicitly includes a compatibility alias.

`--replace` creates an exact set and is reserved for a new empty channel or a
user-authorized removal/reorder. Never use it to add one story to an existing
channel.

A pre-existing version directory is immutable: reuse it only when a checksum
comparison proves it identical; never overwrite different bytes under the same
version.

Before network mutation, verify locally:

- when an APK is required, Player build succeeded and the artifact exists;
- application ID, product label, version/build number, INTERNET permission,
  icon reference, and APK signature match the requested release;
- channel manifest retains all unmodified public entries and contains the
  intended ordered story versions;
- each selected tree contains its card, cover, target-platform preview/release,
  and all referenced payloads;
- artifact sizes and SHA-256 values are recorded.

## Publish without broad deletion

First perform read-only remote checks for authentication, available capacity,
and collisions at every exact final path. Never print private keys, secret
values, verbose authentication traces, or sensitive environment contents.

Immediately before the first external write, obtain explicit user confirmation
of the exact destination and payload: SSH host/account, shared content root,
final APK path, channel manifest path, and versioned story roots. General
permission to "publish to the server" does not substitute for this concrete
confirmation when the execution environment requires it.

Upload each artifact to a unique temporary sibling on the same remote
filesystem. After successful transfer, atomically rename:

1. each new immutable story version into its final version directory;
2. the channel manifest into the shared
   `<remote-root>/<app-id>-<environment>.json`;
3. when changed, the APK into its agreed download path.

Do not use a broad `rsync --delete`, remove old story versions, replace another
app's APK, clean the remote root, or mirror a whole channel-specific story tree.
If a final mutable file already exists, create a recoverable backup when
practical and report the rollback path. If an immutable story version already
exists, checksum-compare it instead of replacing it.

## Verify and hand off

Prove the deployed result from both sides:

- compare local and remote SHA-256 for the channel manifest and, when changed,
  the APK;
- run a checksum dry-run for every published story tree;
- fetch the public channel JSON after the atomic switch and confirm the exact
  ordered story map, including every retained entry;
- require HTTP success and expected content type/length for every manifest
  story's card and target-platform release, plus each changed story's cover,
  preview, and at least one release-referenced payload; check the APK only when
  it changed;
- report signing mode explicitly, especially test signing.

Finish the SomeGame coordination record and release all checkout/resource locks.
The final response should give the download URL, app/package identity, channel,
complete story map, manifest SHA-256, whether the APK was reused or rebuilt,
and, when rebuilt, its size/SHA/signing status. Include any validation that
could not complete. Never claim publication until public checks pass.
