# Official Unity MCP fallback helper

Dependency-free local fallback for Codex Desktop builds that load the Official
Unity MCP configuration but do not expose its tools. It is development tooling;
it is not included in the Unity runtime or Player builds.

## Safety contract

- The checked-in manifest is the only tool allowlist. It initially contains
  read-only `editor_status`, `get_scene_hierarchy`, and `console`.
- Unknown tools fail closed. A tool marked `write` is rejected unless
  `--agent-id` owns the lock below the selected `--coordination-root`.
- The helper never acquires, breaks, or deletes a project lock itself.
- It does not launch Unity Editor. Start Editor only after following the project
  FIFO/write-lock protocol.
- Responses are capped and logs omit arguments/results.

## Start one persistent session

Open Unity through the repository FIFO/write-lock workflow and wait until
`Library/Pipeline/.unity-pipeline-port` exists. Start the helper promptly after
the Editor reports ready; the experimental Unity CLI can discard an old
descriptor whose heartbeat was never refreshed. A clean Editor restart
recreates it.

```bash
cd /Users/iantonishin/Fork/SomeGame
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels serve
```

Use another terminal for short calls:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels status
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels editor-status
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels hierarchy
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels console
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels editor-check
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels benchmark --iterations 30
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels stop
```

`benchmark` performs 30 cycles of the three allowlisted read-only tools in the
same daemon session. Its JSON report includes success rate, median/p95/max
latency, MCP handshake count, paired raw/summary response sizes, estimated
response-size reduction, and whether Git state changed during the run. Use
`--report /absolute/path/report.json` to retain the result outside the repository.
The size reduction is a deterministic character-count proxy, not an exact model
token count.

## Lock-gated compilation

Start the daemon with the exact agent that owns the project write-lock, then use
the bounded compile workflow:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels --agent-id unity-mcp-compile-workflow serve
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels compile --timeout 180
```

`compile` reads Console errors, triggers `recompile`, polls `recompile_status`
until `completed` or `up_to_date`, reads Console errors again, and compares Git
state. The write command fails closed unless the daemon's `--agent-id` matches
the current `write-lock/owner.md`. Polling is bounded; failed/interrupted states
and new Console errors produce a non-success result. This workflow does not edit
scripts, scenes, prefabs, or project settings itself.

## One-call Editor gate

`editor-check` replaces repeated model-driven polling with one bounded local
workflow. Read-only mode returns Editor readiness, active scene/dirty state,
Console cursor and only failure-relevant entries:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels editor-check --console-cursor 120
```

Under the exact coordination lock owner it can also compile and run one
filtered EditMode suite inside the same command:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels --coordination-root . --agent-id your-agent-id \
  editor-check --compile --test-filter Novels.Catalog --filter-type assembly
```

The helper performs internal polling and emits one compact JSON result.
`INITIALIZATION_FAILED`, `fallback.used`, `FATAL EXCEPTION` and Unity error
entries make the gate non-successful. Raw Console/build logs stay on disk and
are opened only for a failed gate.

All Unity projects share the repository queue rooted in `SomeGame`. From the
repository root, pass `--coordination-root .` explicitly for every
write-capable workflow:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Projects/novels-tzm \
  --coordination-root . \
  --agent-id your-agent-id serve
```

Without `--coordination-root`, the helper preserves standalone-project behavior
and looks below `--project`. A missing or mismatched owner always fails closed.

## Lock-gated EditMode tests

With the daemon running under the current lock owner:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels list-tests \
  --arguments '{"mode":"editor"}'
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels editmode-tests \
  --filter Novels.Catalog --filter-type assembly --timeout 300
```

`editmode-tests` always forces `mode=editor`, excludes explicit tests, starts
the run asynchronously, and polls `test_status` with a finite deadline. It
reports the summary, compact failed-test details, Console error delta, and Git
state delta. `run_tests` is write-class and therefore requires the daemon's
`--agent-id` to own the project lock; `list_tests` and `test_status` are
read-only. Filters are JSON arguments, never interpolated into a shell command.
An empty suite is reported as `outcome=no_tests`, `noTests=true`, and a
non-success exit code; it is never presented as a passing quality gate.

Custom arguments remain allowlisted by tool name:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py --project Novels \
  call console --arguments '{"level":"error"}'
```

The default `--format summary` parses the JSON text returned by Unity, removes
Console stack traces, and reduces hierarchy nodes to stable review fields. Use
`--format json` only when the raw MCP envelope is needed. `--max-chars` applies
a hard response cap in either mode.

## Failure model

Failures return JSON with a stable `error.code`: `configuration_error`,
`transport_error`, `startup_timeout`, `tool_timeout`, `invalid_response`, `protocol_incompatible`,
`tool_not_allowed`, `lock_not_owned`, `unity_tool_error`, `compile_failed`,
`compile_timeout`, `tests_failed`, `tests_timeout`, or `internal_error`.
Transport failures are explicitly marked retryable; the caller decides whether
to restart the session, so retries can never loop forever.

## Tests

```bash
python3 -m unittest discover -s Tools/unity-mcp-helper/tests -v
```

The fake MCP tests require neither Unity nor network access. A real Editor smoke
must run sequentially under the repository coordination lock.

## Removal condition

Delete this fallback after Codex Desktop exposes the configured `unity_novels`
namespace reliably across new tasks, honors its allowlist/approval policy, and
passes equivalent reconnect and Editor smoke checks.
