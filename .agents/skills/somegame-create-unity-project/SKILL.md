---
name: somegame-create-unity-project
description: Create and register a new Unity project inside SomeGame, including mandatory per-project Official Unity MCP configuration and live connectivity proof. Use for new story, catalog, tooling, prototype, or other Unity project roots in this repository; do not use for editing an existing project.
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
`Projects/novels-content-template`; story design and content remain the
responsibility of `$somegame-create-story`.

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

## Make Unity MCP mandatory

A new project is incomplete until its own Unity MCP connection is configured
and proven live. Apply the canonical `Docs/AI/guides/UnityMcpWorkflow.md`
protocol with these invariants:

1. Detect providers in the created project. Reuse the single Official Unity
   Pipeline inherited from the canonical template; do not add a second bridge.
   If no provider exists, or a different provider is required, stop for the
   explicit provider/package decision before changing Unity packages.
2. Add a unique optional Codex MCP server entry for this exact project without
   replacing any existing server. Derive a stable name such as
   `unity_novels_<storyId>`, set the literal absolute `--project-path`, and use
   `required = false` for atomic projects whose Editor may normally be closed.
3. Restart Codex Desktop when needed for the new native namespace to appear.
   Configuration text alone is not connection evidence.
4. Under the shared Unity FIFO/write-lock, open only the exact target project
   and verify the Pipeline transport resolves to that same literal path.
5. Run one low-risk live probe (`editor_status`, then hierarchy/Console when
   required), wait for compilation and domain reload to finish, and prove the
   Editor is ready. Use the checked-in persistent helper with explicit
   `--coordination-root .` and matching `--agent-id` when the native namespace
   is unavailable.
6. Perform the documented restart/reconnect check and confirm the probe leaves
   no unexplained Git delta. Close the helper and Editor unless the user asked
   to keep them open.

Do not weaken this requirement to package presence, a port file, a server
entry, or a claim that another SomeGame project's MCP works. If live proof is
blocked by unavailable Editor/client restart/user approval, report the project
as scaffolded but not ready and state the exact remaining gate.

## Validate and hand off

Use the current changed-path plan and the project-kind-specific validator or
build command. For a story project, validate its marker/config and run the
canonical story validation once minimum playable content exists; do not invent
placeholder narrative merely to make validation green.

The handoff must identify the new project root, template and deliberate
deviations, Unity version, MCP provider/package, unique server name, exact
project path, live/restart probe evidence, validation results, Git delta, and
any remaining manual or publication step. Do not declare the project ready
while the mandatory MCP gate is unresolved.
