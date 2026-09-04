# Agent: `child-bubble-alpha-skill`

- Status: ready-for-integration
- Task: supplement the child-story Bubble skill with the proven transparent-edge and Android texture-compression safeguards.
- Scope: `.agents/skills/somegame-create-child-story-bubbles/SKILL.md`, skill validation evidence, `Docs/AI/CoordinationRuntime/HANDOFF.md`, own coordination records.
- Base: `f691f613`.

## Result

- Added explicit alpha-channel validation: zero-alpha exterior, one clean connected panel/card shape, and no patterned or isolated edge pixels.
- Added hidden-RGB matte hygiene to prevent bilinear and compression colour bleed.
- Added a bounded recovery rule for thresholded masks and alpha-capable lossless/uncompressed per-platform overrides on small UI sprites.
- The runtime gate now requires rebuilt atomic content and Player evidence against the real scene, rejecting white rims, checkerboards, speckles and halos.
- `Tools/somegame verify` passed. The bundled `quick_validate.py` could not start because both available Python runtimes lack its undeclared `PyYAML` dependency; an equivalent Ruby YAML/frontmatter/name/TODO validation passed.
- `Tools/somegame docs-check` passed all substantive checks but reports the pre-existing `HANDOFF.md` rotation limit (155 lines before this entry); rotation was not expanded into this narrow skill task.
