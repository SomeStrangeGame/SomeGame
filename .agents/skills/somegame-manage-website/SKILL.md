---
name: somegame-manage-website
description: Build, adapt, validate, publish, or roll back the SomeGame/Kostroma static story website. Use for landing-page and story-reader UI changes, responsive browser checks, search-indexing controls, access splash screens, and SSH deployment of versioned site assets; not for authoring story preview content or releasing the Unity application.
---

# Manage the SomeGame website

Treat `Website/` as the website source and preserve its separate Git state. Follow the
repository coordination protocol before changing it. Use the Sites building guidance
for implementation and responsive review, then the Sites hosting guidance for actual
publication. Website-only work does not require Unity.

Keep three concerns separate:

- the site shell and reader UI;
- story-owned data under versioned `content/stories/<story>/<version>/` trees;
- app-channel manifests and downloadable application builds.

Changing one does not authorize or imply publishing the others. For producing a new
story preview, use the story-creation handoff in
`../somegame-create-story/references/web-preview-publication.md`. For building or
publishing an application, use `$somegame-release-app`.

## Workflow

1. Inspect the current source, live release and relevant manifest before editing. Do
   not infer the live structure from a stale temporary deployment directory.
2. Make the smallest source change in `Website/`. Keep content and presentation
   contracts data-driven; do not embed story prose, annotations, covers, or character
   lists in the site shell.
3. Run the production site build and syntax/schema checks proportional to the change.
4. Inspect the actual UI at every affected breakpoint. Responsive work normally needs
   both portrait phone and short landscape phone checks, plus desktop. Exercise real
   scrolling, character changes, modal close paths and fixed download controls when
   those surfaces are affected.
5. Before an external write, resolve the exact payload, release ID, destination and
   rollback target and obtain the required current authorization. Publication
   permission is not inherited from an earlier unrelated release.
6. Publish assets into a new immutable release directory, switch the root HTML last
   with an atomic same-directory rename, and verify the public HTTPS result.
7. Record the live release ID and meaningful verification evidence in the task handoff.

Read [references/hosting.md](references/hosting.md) for the current server topology,
atomic deployment contract, access-gate caveat and release checklist whenever the task
touches hosting or the live site.

## Non-negotiable checks

- Preserve unknown local and server-side files; never replace the whole web root.
- Never mutate an older release in place. A correction gets a new release ID.
- Parse deployed JavaScript and validate changed JSON before upload.
- Confirm root HTML references only the intended release.
- Check representative assets and all changed public URLs for successful responses.
- After visual changes, inspect the live page rather than treating the build as visual
  proof.
- A client-side password splash is only an obscurity layer. Do not describe it as real
  authentication; recommend server-side access control when confidentiality matters.
- Search exclusion should use both page-level robots metadata and a root `robots.txt`.
  It is a crawler request, not a secrecy mechanism, and does not remove already indexed
  URLs by itself.
