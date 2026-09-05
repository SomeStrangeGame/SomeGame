---
name: somegame-create-unity-project
description: Create and register a new Unity project inside SomeGame, including per-project Official Unity MCP configuration with live proof deferred to the repository validation workflow. Use for new story, catalog, tooling, prototype, or other Unity project roots in this repository; do not use for editing an existing project.
---

# Create a SomeGame Unity project

Use this skill together with `$somegame-workflow` and
`$unity-workbench:unity-mcp-workflow`. Current repository documentation,
templates, tooling, and coordination state are authoritative.

## Define the project contract

Before writing, establish the project kind and stable identifier, exact target
path inside the SomeGame Git root, canonical template or closest existing
project, expected output, and the smallest validation that proves it works.

Reject paths outside the repository, collisions with an existing project, and
identifiers that do not follow the selected project kind's current convention.
For an atomic story, use `Projects/novels-<storyId>` and derive it from
`Projects/novels-content-template`. `$somegame-create-story` owns orchestration;
`$somegame-design-story` and `$somegame-author-story-content` own their
respective production stages.

## Create the project safely

Inspect the complete dirty tree and active scopes before declaring the exact
new-project path. Follow SomeGame FIFO/write-lock rules for every write,
generator, Unity launch, import, build, or validation operation. Do not switch
branches, copy generated `Library`, `Temp`, `Logs`, or build output, or absorb
unrelated changes.

Copy only the maintained source/configuration surface from the selected
template. Preserve its Unity version, repository-relative local package paths,
and compatible package versions unless the task explicitly requires a
migration. Replace template identifiers and product metadata intentionally;
never use a blind repository-wide substitution. Confirm the new root contains
at least `Assets/`, `Packages/manifest.json`, and
`ProjectSettings/ProjectVersion.txt`.

## Configure Unity MCP and defer live proof

Configure the project through `Docs/AI/guides/UnityMcpWorkflow.md`, which
exclusively owns provider selection, server-entry shape, live probes and restart
evidence. Project creation only prepares the per-project configuration and must
report `scaffolded; MCP configured; live proof deferred`. The final live proof
follows `UnityConcurrency.md` and is not repeated or redefined here.

## Validate and hand off

Use dependency-free checks for project structure, marker/config text and local
package paths. Project creation does not run deferred Unity-backed gates or
invent placeholder narrative merely to make static validation green.

The handoff must identify the new project root, template and deliberate
deviations, Unity version, MCP provider/package, unique server name, exact
project path, deferred live/restart probe, static validation results, Git delta,
and any remaining manual or publication step. Use
`ready-for-final-validation`, not live-ready, until the mandatory MCP proof
passes in the authorized final slot.
