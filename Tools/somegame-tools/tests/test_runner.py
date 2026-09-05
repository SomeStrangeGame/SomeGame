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

    def test_android_sdk_controller_retry_is_documented_as_benign(self):
        diagnostic = runner.KNOWN_BENIGN_ANDROID_DIAGNOSTICS[0]
        self.assertEqual("127.0.0.1:1970", diagnostic["endpoint"])
        self.assertFalse(diagnostic["affectsGate"])
        proxifier_line = (
            "qemu-system-aarch64 - 127.0.0.1:1970 error: "
            "Could not connect to 127.0.0.1:1970 - connection failed with error 61"
        )
        self.assertFalse(runner.android_log_has_blocking_marker(proxifier_line))

    def test_real_android_failure_remains_blocking(self):
        self.assertTrue(runner.android_log_has_blocking_marker("FATAL EXCEPTION: main"))

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

    def test_player_parser_accepts_test_signing(self):
        args = runner.parser().parse_args([
            "player-build", "--agent-id", "a", "--target", "Android",
            "--mode", "Embedded", "--test-signing",
        ])
        self.assertTrue(args.test_signing)
        self.assertFalse(args.development)

    def test_player_parser_accepts_children_catalog_variant(self):
        args = runner.parser().parse_args([
            "player-build", "--agent-id", "a", "--target", "Android",
            "--mode", "Embedded", "--catalog-variant", "children",
        ])
        self.assertEqual("children", args.catalog_variant)

    def test_player_parser_accepts_scp_catalog_variant(self):
        args = runner.parser().parse_args([
            "player-build", "--agent-id", "a", "--target", "Android",
            "--mode", "Embedded", "--catalog-variant", "scp",
        ])
        self.assertEqual("scp", args.catalog_variant)

    def test_player_parser_accepts_nochelessie_catalog_variant(self):
        args = runner.parser().parse_args([
            "player-build", "--agent-id", "a", "--target", "Android",
            "--mode", "Embedded", "--catalog-variant", "nochelessie",
        ])
        self.assertEqual("nochelessie", args.catalog_variant)

    def test_player_parser_rejects_development_with_test_signing(self):
        with self.assertRaises(SystemExit):
            runner.parser().parse_args([
                "player-build", "--agent-id", "a", "--target", "Android",
                "--mode", "Embedded", "--development", "--test-signing",
            ])

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

    def test_context_parser_defaults_to_code(self):
        args = runner.parser().parse_args(["context"])
        self.assertEqual("code", args.task)

    def test_context_parser_accepts_owned_paths_and_inspect(self):
        args = runner.parser().parse_args(["context", "--task", "inspect", "--resume",
                                           "--paths", "Docs/AI/README.md"])
        self.assertEqual(["Docs/AI/README.md"], args.paths)
        self.assertTrue(args.resume)

    def test_queue_status_parser_accepts_agent(self):
        args = runner.parser().parse_args(["queue-status", "--agent-id", "agent-a"])
        self.assertEqual("agent-a", args.agent_id)

    def test_queue_prune_removes_only_terminal_orphan(self):
        previous_root = runner.ROOT
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); runtime = root / "Docs/AI/CoordinationRuntime"
            request = runtime / "requests/20260101T000000Z-done"
            agent = runtime / "agents/done.md"
            request.mkdir(parents=True); agent.parent.mkdir(parents=True)
            (request / "request.md").write_text(
                "- Agent: `done`\n- Status: queued\n- Requested UTC: `2026-01-01T00:00:00Z`\n")
            agent.write_text("- Status: completed\n")
            try:
                runner.ROOT = root
                args = runner.parser().parse_args([
                    "queue-prune", "--request", "20260101T000000Z-done"])
                result = runner.queue_prune_workflow(args)
                self.assertTrue(result["ok"])
                self.assertFalse(request.exists())
            finally:
                runner.ROOT = previous_root

    def test_queue_status_degrades_when_process_probe_is_unavailable(self):
        previous_root = runner.ROOT
        previous_processes = runner.unity_processes
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); runtime = root / "Docs/AI/CoordinationRuntime"
            (runtime / "requests").mkdir(parents=True)
            try:
                runner.ROOT = root
                runner.unity_processes = lambda: (_ for _ in ()).throw(PermissionError("denied"))
                result = runner.queue_status_workflow(runner.parser().parse_args(["queue-status"]))
                self.assertTrue(result["ok"])
                self.assertEqual("unavailable", result["coordination"]["processProbe"])
                self.assertIsNone(result["coordination"]["heavyProcesses"])
            finally:
                runner.ROOT = previous_root
                runner.unity_processes = previous_processes

    def test_refresh_lock_heartbeat_only_for_owner(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); lock = root / "Docs/AI/CoordinationRuntime/active/write-lock"
            lock.mkdir(parents=True); owner = lock / "owner.md"
            owner.write_text("- Agent: `agent-a`\n- Heartbeat UTC: `2026-01-01T00:00:00Z`\n")
            previous = runner.ACTIVE_AGENT_ID
            try:
                runner.ACTIVE_AGENT_ID = "agent-b"
                self.assertFalse(runner.refresh_lock_heartbeat(root))
                runner.ACTIVE_AGENT_ID = "agent-a"
                self.assertTrue(runner.refresh_lock_heartbeat(root))
                self.assertNotIn("2026-01-01T00:00:00Z", owner.read_text())
            finally:
                runner.ACTIVE_AGENT_ID = previous

    def test_verify_scopes_diff_check_to_owned_paths(self):
        args = runner.parser().parse_args(["verify", "--explain", "--paths", "owned.md"])
        plan = {"run_helper_tests": False, "run_automation_tests": False,
                "content_targets": [], "editor_compile": False, "editmode_tests": False,
                "player_build": False, "manual_visual_gate": False}
        commands, _ = runner.verification_commands(plan, args)
        self.assertEqual(["git", "diff", "--check", "--", "owned.md"], commands[0][1])

    def test_verify_explain_does_not_require_agent(self):
        args = runner.parser().parse_args(["verify", "--explain", "--paths", "Docs/AI/README.md"])
        self.assertTrue(args.explain)
        self.assertIsNone(args.agent_id)

    def test_editor_no_stop_uses_detached_process_session(self):
        args = runner.parser().parse_args([
            "editor-gate", "--agent-id", "a", "--no-stop-editor",
        ])
        self.assertFalse(args.stop_editor)
        self.assertEqual(
            {"start_new_session": True},
            runner.editor_process_options(args.stop_editor))

    def test_editor_default_keeps_managed_process_session(self):
        args = runner.parser().parse_args(["editor-gate", "--agent-id", "a"])
        self.assertTrue(args.stop_editor)
        self.assertEqual(
            {"start_new_session": False},
            runner.editor_process_options(args.stop_editor))

    def test_compiler_error_scan_detects_csharp_errors(self):
        with tempfile.TemporaryDirectory() as directory:
            log = Path(directory) / "Editor.log"
            log.write_text(
                "normal line\nAssets/Foo.cs(3,4): error CS0246: Missing type\n",
                encoding="utf-8")
            self.assertEqual(1, len(runner.compiler_error_lines(log)))

    def test_compiler_error_scan_ignores_normal_log(self):
        with tempfile.TemporaryDirectory() as directory:
            log = Path(directory) / "Editor.log"
            log.write_text("CompilationPipeline: compilation finished\n", encoding="utf-8")
            self.assertEqual([], runner.compiler_error_lines(log))

    def test_repeating_workflow_parsers(self):
        start = runner.parser().parse_args([
            "start-task", "--agent-id", "my-task", "--task", "Do work", "--scope", "Tools/x"])
        self.assertEqual("my-task", start.agent_id)
        story = runner.parser().parse_args([
            "story-check", "--agent-id", "a", "--target", "tzm", "--build", "--platform", "android"])
        self.assertTrue(story.build)
        cycle = runner.parser().parse_args([
            "android-dev-cycle", "--agent-id", "a", "--package-id", "com.example.game"])
        self.assertEqual("emulator-5554", cycle.serial)
        clean = runner.parser().parse_args([
            "clean-generated", "--agent-id", "a", "--project", "Novels"])
        self.assertFalse(clean.apply)

    def test_clean_generated_is_dry_run_by_default(self):
        previous_root = runner.ROOT
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "Projects/novels-test"
            (project / "ProjectSettings").mkdir(parents=True)
            (project / "Library").mkdir()
            lock = root / "Docs/AI/CoordinationRuntime/active/write-lock"
            lock.mkdir(parents=True)
            (lock / "owner.md").write_text("- Agent: `a`\n", encoding="utf-8")
            try:
                runner.ROOT = root
                args = runner.parser().parse_args([
                    "clean-generated", "--agent-id", "a", "--project", "Projects/novels-test"])
                result = runner.clean_generated(args)
                self.assertTrue(result["dryRun"])
                self.assertTrue((project / "Library").is_dir())
            finally:
                runner.ROOT = previous_root

    def test_clean_generated_rejects_repository_root(self):
        previous_root = runner.ROOT
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "ProjectSettings").mkdir()
            lock = root / "Docs/AI/CoordinationRuntime/active/write-lock"
            lock.mkdir(parents=True)
            (lock / "owner.md").write_text("- Agent: `a`\n", encoding="utf-8")
            try:
                runner.ROOT = root
                args = runner.parser().parse_args([
                    "clean-generated", "--agent-id", "a", "--project", "."])
                with self.assertRaises(runner.WorkflowError):
                    runner.clean_generated(args)
            finally:
                runner.ROOT = previous_root


if __name__ == "__main__":
    unittest.main()
