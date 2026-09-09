# Website preview and Remote publication handoff

Read this reference when producing a new story's site preview or when an
end-to-end creation request also includes deployment. It defines the handoff;
`$somegame-release-app`, current repository documentation and release tooling
remain authoritative for builds and external writes.

## Produce a story-owned reading preview

Every new story provides `Config/Preview/preview.json` with schema version 1 and
only the referenced files below `Config/Preview/characters/`. Keep `storyId`,
episode ID, title, character IDs and image paths stable and internally
consistent. Use narration, dialogue, separator and character-stage blocks that
the current website contract supports.

Select a short linear excerpt from the canonical opening Ink. Copy prose and
dialogue faithfully, preserve their order and speaker attribution, stop before
a meaningful spoiler or unresolved choice, and do not invent bridging text to
make the excerpt read differently from the game. Include only characters shown
by the excerpt and use approved story masters adapted for site delivery without
changing identity. `Remote/<platform>/catalog-preview.json` is generated catalog
metadata and is not the website reading preview.

Validate JSON/schema, canonical IDs, safe relative paths, referenced file
existence, character-marker coverage, and source-text correspondence. When the
site is part of the request, also inspect the real preview in its supported
portrait and landscape layouts, including scrolling, character transitions,
close/backdrop/Escape behavior and the final continuation message.

## Prepare release inputs

After acceptance and integration, resolve rather than assume:

- the app profile and Remote platform;
- dev or prod channel and exact ordered `story-id=version` set;
- common HTTPS content root and exact remote/public paths;
- whether the installed Player's embedded catalog registry contains the story
  and its runtime understands the current manifest/content schemas;
- signing and audience requirements if an APK must be rebuilt.

The website and Remote Player may read the same channel manifest, but publishing
a website card or preview does not register a story in the application. The
application discovers the selected versions through
`<app-id>-<environment>.json` and reads content from
`stories/<story-id>/<version>/`.

A new APK is unnecessary only when the already distributed compatible Remote
Player embeds a catalog registry that contains the new story and no runtime,
schema, profile or platform requirement changed. Otherwise rebuild and publish
the Player through `$somegame-release-app`; updating the channel manifest alone
cannot add a card unknown to its embedded registry.

## Publish safely

Invoke `$somegame-release-app` for the actual release. Its approval, locking,
build, signing, staging, upload, rollback and verification rules apply. In
particular, auto-approve or end-to-end story creation does not authorize server
mutation. Resolve the exact destination and payload and obtain the required
current confirmation immediately before the first external write.

The safe visibility order is:

1. build and verify the exact story and catalog outputs and, when required, the
   compatible Remote Player;
2. stage and verify the exact ordered channel locally;
3. upload the new immutable `stories/<story-id>/<version>/` tree without
   replacing different bytes at an existing version;
4. publish its story-owned `preview/preview.json` and referenced character
   images inside that version tree, plus any separately versioned site release;
5. atomically replace `<app-id>-<environment>.json` last so clients never see a
   partially uploaded story;
6. verify hashes and public HTTP responses for the manifest, card, cover,
   platform release, representative payload and website preview, then perform
   the required real Remote Player/catalog smoke.

Preserve every previously selected story unless the release contract explicitly
removes it. Do not publish a generic `dev.json`/`prod.json` alias, delete older
versions, or treat a successful site deployment as proof that the application
can load the story.
