# Agent: `somegame-manage-website-skill`

- Status: completed
- Task: Create reusable project skill for building, validating, and atomically publishing the static SomeGame website
- Scope: .agents/skills/somegame-manage-website/**; own coordination and compact handoff only. No Website, story, runtime, server, or Unity changes.
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T15:54:23Z`.
- Result: created project skill `somegame-manage-website` with a concise routing entrypoint, current static-hosting topology, immutable atomic SSH release/rollback protocol, responsive live-browser verification, crawler exclusion and client-side access-gate caveats. YAML parsing, placeholder/secret scan and scoped diff checks passed; the bundled Python validator could not start because PyYAML is absent.
