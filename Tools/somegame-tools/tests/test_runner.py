import importlib.util
import json
import sys
import tempfile
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "runner.py"
SPEC = importlib.util.spec_from_file_location("somegame_runner", MODULE_PATH)
runner = importlib.util.module_from_spec(SPEC)
assert SPEC.loader
sys.modules[SPEC.name] = runner
SPEC.loader.exec_module(runner)


class RunnerTests(unittest.TestCase):
    def test_parse_smoke_events_ignores_invalid_lines(self):
        text = "noise\n[NOVELS_SMOKE] " + json.dumps({"v": 1, "seq": 2, "runId": "r", "event": "catalog.ready"})
        self.assertEqual(["catalog.ready"], [item["event"] for item in runner.parse_smoke_events(text)])

    def test_parse_smoke_events_accepts_unity_rich_text_suffix(self):
        text = ('I Unity : [NOVELS_SMOKE] '
                '{"v":1,"seq":1,"runId":"r","event":"app.started"}</color>')
        self.assertEqual(["app.started"], [item["event"] for item in runner.parse_smoke_events(text)])

    def test_event_sequence_is_ordered(self):
        events = [{"runId": "r", "event": value} for value in ("app.started", "catalog.ready", "episode.ready")]
        self.assertEqual((True, []), runner.event_sequence_ok(events, ["app.started", "episode.ready"]))
        self.assertEqual((False, ["app.started"]), runner.event_sequence_ok(events, ["episode.ready", "app.started"]))

    def test_markdown_scan_detects_broken_target(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); docs = root / "Docs/AI"; docs.mkdir(parents=True)
            (docs / "test.md").write_text("[missing](nope.md)\n", encoding="utf-8")
            self.assertEqual("missing_target", runner.scan_markdown(root)[0]["reason"])

    def test_lock_owner_requires_exact_agent(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); lock = root / "Docs/AI/CoordinationRuntime/active/write-lock"
            lock.mkdir(parents=True); (lock / "owner.md").write_text("- Agent: `agent-a`\n", encoding="utf-8")
            self.assertEqual("agent-a", runner.lock_owner(root))

    def test_default_player_output_has_target_suffix(self):
        self.assertEqual("Novels.apk", runner.default_player_output("Android", "Embedded").name)

    def test_content_gate_parser_accepts_explicit_target(self):
        args = runner.parser().parse_args(["content-gate", "--agent-id", "a", "--target", "catalog"])
        self.assertEqual("catalog", args.target)

    def test_git_publish_parser_has_safe_defaults(self):
        args = runner.parser().parse_args(["git-publish", "--agent-id", "a"])
        self.assertEqual(("origin", "main", None), (args.remote, args.branch, args.ssh_key))

    def test_git_publish_allows_only_untracked_own_runtime_records(self):
        allowed = {"owner.md", "request.md", "agent.md"}
        lines = ["?? owner.md", "?? request.md", "?? agent.md", " M tracked.md", "?? other.md"]
        self.assertEqual([" M tracked.md", "?? other.md"],
                         runner.unexpected_git_status(lines, allowed))

    def test_git_publish_rejects_staged_runtime_record(self):
        self.assertEqual(["A  owner.md"],
                         runner.unexpected_git_status(["A  owner.md"], {"owner.md"}))


if __name__ == "__main__":
    unittest.main()
