# Static website hosting protocol

Read this reference only for live publication, rollback, or hosting diagnosis.

## Current topology

- Public origin: `https://pureshechka.com/`.
- Remote web root: `/home/p/pureshecom/public_html`.
- Versioned site assets: `site/releases/<release-id>/`.
- Root `index.html` is the switch point and references the selected release's
  `styles.css` and `app.js`.
- Story cards and previews are not copied into the site release. They remain below
  `content/stories/<story-id>/<version>/` and are selected by the Kostroma channel
  manifest.
- Downloadable app builds remain below `DevBuilds/` or their configured production
  destination; a site release must not silently replace them.

Resolve SSH identity and key location from the current approved release configuration
or task context. Paths to credentials may be recorded in local configuration, but
credential contents and passwords must never be copied into this skill, logs, commands
or handoff notes.

## Prepare the payload

Use a fresh temporary directory created for the task. Assemble the complete static
payload needed by the current shell; do not depend on a previous task's `/private/tmp`
directory. At minimum verify:

- `index.html` has the intended release references and metadata;
- `app.js` parses successfully;
- `styles.css` contains the source changes;
- any root files such as `robots.txt` are explicitly included when changed;
- asset and content URLs remain absolute from the origin and point at existing files.

The repository's production build must pass even when the hosting shell is currently a
separate static bundle. Keep source and deployed behavior aligned. If this bridge grows
beyond a small mechanical assembly, replace it with a deterministic export script
rather than documenting more manual steps.

## Publish atomically

1. Choose a new, unused, sortable release ID.
2. Create `site/releases/<release-id>/` without changing the active root page.
3. Upload the complete changed site asset set and verify sizes or hashes remotely.
4. Upload root switch files with a `.next` suffix in the same directory as their final
   targets.
5. Rename ancillary root files first when necessary, then rename `index.html.next` to
   `index.html` last. A same-filesystem rename is the visibility switch.
6. Keep the previous release intact for rollback. Do not delete historical releases as
   part of an ordinary deploy.

Never assume a release directory contains `index.html`; in the current layout the root
page is separate. Inspect the remote tree before composing commands. Avoid a multi-step
command whose early copy can succeed while a later assumed path fails without being
noticed.

## Verify

After the switch:

- fetch `/` and confirm it references only the new release ID;
- fetch the new CSS and JS endpoints and representative story/card/preview assets;
- fetch `/robots.txt` when indexing rules changed;
- inspect browser console errors;
- open the live site in desktop, portrait phone and short landscape phone layouts;
- test the changed interaction through its final visible state, not only its initial
  render;
- for an access splash, test rejection, acceptance, refresh, and new-tab/session
  behavior without exposing the password in evidence.

## Roll back

Rollback changes the root switch point back to the last known-good immutable release;
it does not edit either release. Prepare the prior root HTML as `index.html.next`, verify
its references, and atomically rename it into place. Recheck `/` and its referenced
assets after rollback.
