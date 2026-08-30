# gpl-missing-setting-screen

Status: ready-for-integration
Base: 4bfd64af41d3
Scope:
- `Projects/novels-gpl/Assets/Presentation/**`
- generated GPL local content under `Novels/Build/LocalContent/stories/gpl/**`
- own coordination records and compact handoff entry

Task: restore the mandatory story setting screen asset missing from the GPL bundle.
Validation: GPL content validate/build, Unity compile, original story-open reproduction.

Result: added the mandatory setting prefab with built-in Unity font dependency; GPL
release `9fcb8d0578df48f8e9df5d78b51832ad70a8f51fa9892ff801835d7b77612f56`
lists the requested address in chunk 0. Validate/build and fresh Novels compile passed.
Pending: one manual click of the GPL card to confirm the original runtime path.
