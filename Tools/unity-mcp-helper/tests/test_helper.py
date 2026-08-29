import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from unity_mcp_helper import (HelperError, McpProcess, Policy, percentile, run_call,
                              run_compile, run_editmode_tests, run_editor_check)


class HelperTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        manifest = {"protocolVersion": "2025-06-18", "tools": {
            "editor_status": {"access": "read", "timeoutSec": 1},
            "slow": {"access": "read", "timeoutSec": 0.05},
            "malformed": {"access": "read", "timeoutSec": 1},
            "crash": {"access": "read", "timeoutSec": 1},
            "write_test": {"access": "write", "timeoutSec": 1}}}
        self.manifest = self.root / "manifest.json"
        self.manifest.write_text(json.dumps(manifest), encoding="utf-8")
        self.policy = Policy(self.manifest, self.root, "test-agent")
        self.client = McpProcess([sys.executable, str(ROOT / "tests/fake_mcp.py")], "2025-06-18", 1)

    def tearDown(self):
        self.client.close(); self.temp.cleanup()

    def test_handshake_and_read_call(self):
        self.client.start()
        result = run_call(self.client, self.policy, "editor_status", {}, self.root / "events.jsonl")
        self.assertTrue(result["ok"])
        self.assertEqual(result["tool"], "editor_status")

    def test_unknown_tool_is_denied(self):
        with self.assertRaises(HelperError) as context:
            self.policy.authorize("delete_everything")
        self.assertEqual(context.exception.code, "tool_not_allowed")

    def test_write_without_owned_lock_is_denied(self):
        with self.assertRaises(HelperError) as context:
            self.policy.authorize("write_test")
        self.assertEqual(context.exception.code, "lock_not_owned")

    def test_write_with_owned_lock_is_allowed(self):
        owner = self.root / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
        owner.parent.mkdir(parents=True)
        owner.write_text("- Agent: `test-agent`\n", encoding="utf-8")
        self.assertEqual(self.policy.authorize("write_test")["access"], "write")

    def test_write_with_explicit_coordination_root_is_allowed(self):
        target = self.root / "atomic-project"
        coordination = self.root / "coordination-project"
        owner = coordination / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
        owner.parent.mkdir(parents=True)
        owner.write_text("- Agent: `test-agent`\n", encoding="utf-8")
        policy = Policy(self.manifest, target, "test-agent", coordination)
        self.assertEqual(policy.authorize("write_test")["access"], "write")

    def test_explicit_coordination_root_fails_closed_for_wrong_owner(self):
        coordination = self.root / "coordination-project"
        owner = coordination / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
        owner.parent.mkdir(parents=True)
        owner.write_text("- Agent: `other-agent`\n", encoding="utf-8")
        policy = Policy(self.manifest, self.root / "atomic-project", "test-agent", coordination)
        with self.assertRaises(HelperError) as context:
            policy.authorize("write_test")
        self.assertEqual(context.exception.code, "lock_not_owned")

    def test_timeout_is_classified(self):
        self.client.start()
        with self.assertRaises(HelperError) as context:
            self.client.call("slow", {}, 0.05)
        self.assertEqual(context.exception.code, "tool_timeout")
        self.assertTrue(context.exception.retryable)

    def test_malformed_json_is_classified(self):
        self.client.start()
        with self.assertRaises(HelperError) as context:
            self.client.call("malformed", {}, 1)
        self.assertEqual(context.exception.code, "invalid_response")

    def test_process_crash_is_classified(self):
        self.client.start()
        with self.assertRaises(HelperError) as context:
            self.client.call("crash", {}, 1)
        self.assertEqual(context.exception.code, "transport_error")

    def test_bad_manifest_is_configuration_error(self):
        bad = self.root / "bad.json"
        bad.write_text("{", encoding="utf-8")
        with self.assertRaises(HelperError) as context:
            Policy(bad, self.root, None)
        self.assertEqual(context.exception.code, "configuration_error")

    def test_percentile_uses_bounded_nearest_rank(self):
        self.assertEqual(percentile([], 0.95), 0)
        self.assertEqual(percentile([1, 2, 3, 4, 100], 0.50), 3)
        self.assertEqual(percentile([1, 2, 3, 4, 100], 0.95), 100)

    def test_compile_requires_owned_lock(self):
        with self.assertRaises(HelperError) as context:
            self.policy.authorize("write_test")
        self.assertEqual(context.exception.code, "lock_not_owned")

    def test_compile_workflow_completes_without_new_errors(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": True, "result": {"status": "compiling"}},
            {"ok": True, "result": {"status": "compiling"}},
            {"ok": True, "result": {"status": "completed"}},
            {"ok": True, "result": {"returned": 0}},
        ])
        result = run_compile(self.root, self.root, 1, 0.001,
                             sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertTrue(result["ok"])
        self.assertEqual(result["polls"], 2)
        self.assertEqual(result["newConsoleErrors"], 0)

    def test_compile_workflow_reports_new_errors(self):
        responses = iter([
            {"ok": True, "result": {"returned": 1}},
            {"ok": True, "result": {"status": "triggered"}},
            {"ok": True, "result": {"status": "completed"}},
            {"ok": True, "result": {"returned": 3}},
        ])
        result = run_compile(self.root, self.root, 1, 0.001,
                             sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertFalse(result["ok"])
        self.assertEqual(result["newConsoleErrors"], 2)

    def test_compile_workflow_accepts_up_to_date_trigger_without_polling(self):
        calls = []
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": True, "result": {"status": "up_to_date"}},
            {"ok": True, "result": {"returned": 0}},
        ])
        def sender(_, payload):
            calls.append(payload)
            return next(responses)
        result = run_compile(self.root, self.root, 1, 0.001, sender=sender, sleeper=lambda _: None)
        self.assertTrue(result["ok"])
        self.assertEqual(result["polls"], 0)
        self.assertNotIn("recompile_status", [call.get("tool") for call in calls])

    def test_compile_workflow_rejects_invalid_timeout(self):
        with self.assertRaises(HelperError) as context:
            run_compile(self.root, self.root, 0, 1)
        self.assertEqual(context.exception.code, "configuration_error")

    def test_compile_workflow_preserves_policy_error(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": False, "error": {"code": "lock_not_owned", "message": "denied", "retryable": False}},
        ])
        with self.assertRaises(HelperError) as context:
            run_compile(self.root, self.root, 1, 0.001,
                        sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertEqual(context.exception.code, "lock_not_owned")

    def test_compile_workflow_preserves_retryable_trigger_error(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": False, "error": {"code": "unity_tool_error", "message": "no pipeline", "retryable": True}},
        ])
        with self.assertRaises(HelperError) as context:
            run_compile(self.root, self.root, 1, 0.001,
                        sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertEqual(context.exception.code, "unity_tool_error")
        self.assertTrue(context.exception.retryable)

    def test_editmode_workflow_passes_and_forwards_filter(self):
        calls = []
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": True, "result": {"status": "running"}},
            {"ok": True, "result": {"status": "running"}},
            {"ok": True, "result": {"status": "completed", "summary": {"total": 3, "passed": 3, "failed": 0}}},
            {"ok": True, "result": {"returned": 0}},
        ])
        def sender(_, payload):
            calls.append(payload)
            return next(responses)
        result = run_editmode_tests(self.root, self.root, 1, 0.001, "Catalog", "assembly",
                                    sender=sender, sleeper=lambda _: None)
        self.assertTrue(result["ok"])
        self.assertEqual(result["total"], 3)
        self.assertEqual(calls[1]["arguments"]["filter"], "Catalog")
        self.assertEqual(calls[1]["arguments"]["filter_type"], "assembly")
        self.assertEqual(calls[1]["arguments"]["mode"], "editor")

    def test_editmode_workflow_reports_failed_tests(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": True, "result": {"status": "running"}},
            {"ok": True, "result": {"status": "completed", "summary": {"total": 2, "passed": 1, "failed": 1}}},
            {"ok": True, "result": {"returned": 0}},
        ])
        result = run_editmode_tests(self.root, self.root, 1, 0.001, "", "testName",
                                    sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertFalse(result["ok"])
        self.assertEqual(result["failed"], 1)

    def test_editmode_workflow_does_not_treat_empty_suite_as_passed(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": True, "result": {"status": "completed"}},
            {"ok": True, "result": {"status": "completed", "summary": {"total": 0, "passed": 0, "failed": 0}}},
            {"ok": True, "result": {"returned": 0}},
        ])
        result = run_editmode_tests(self.root, self.root, 1, 0.001, "", "testName",
                                    sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertFalse(result["ok"])
        self.assertTrue(result["noTests"])
        self.assertEqual(result["outcome"], "no_tests")

    def test_editmode_workflow_preserves_policy_error(self):
        responses = iter([
            {"ok": True, "result": {"returned": 0}},
            {"ok": False, "error": {"code": "lock_not_owned", "message": "denied", "retryable": False}},
        ])
        with self.assertRaises(HelperError) as context:
            run_editmode_tests(self.root, self.root, 1, 0.001, "", "testName",
                               sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertEqual(context.exception.code, "lock_not_owned")

    def test_editmode_workflow_rejects_invalid_filter_type(self):
        with self.assertRaises(HelperError) as context:
            run_editmode_tests(self.root, self.root, 1, 1, "", "regex")
        self.assertEqual(context.exception.code, "configuration_error")

    def test_editor_check_aggregates_read_only_state(self):
        calls = []
        responses = iter([
            {"ok": True, "result": {"status": "ready", "unityVersion": "6000.3.11f1",
                                      "isCompiling": False, "playModeState": "stopped"}},
            {"ok": True, "result": {"scenePath": "Assets/Main.unity", "isDirty": False,
                                      "roots": [{"name": "Root"}]}},
            {"ok": True, "result": {"cursor": 12, "returned": 1,
                                      "entries": [{"level": "log", "message": "ready"}]}},
        ])
        def sender(_, payload):
            calls.append(payload)
            return next(responses)
        result = run_editor_check(self.root, self.root, False, None, "testName", 1, 0.001,
                                  sender=sender, sleeper=lambda _: None)
        self.assertTrue(result["ok"])
        self.assertEqual(result["scene"]["rootCount"], 1)
        self.assertEqual(len(calls), 3)

    def test_editor_check_blocks_structured_failure_marker(self):
        responses = iter([
            {"ok": True, "result": {"status": "ready"}},
            {"ok": True, "result": {"scenePath": "Assets/Main.unity", "isDirty": False, "roots": []}},
            {"ok": True, "result": {"cursor": 9, "returned": 1,
                                      "entries": [{"level": "log", "message": "[NOVELS_SMOKE] fallback.used"}]}},
        ])
        result = run_editor_check(self.root, self.root, False, None, "testName", 1, 0.001,
                                  sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertFalse(result["ok"])
        self.assertEqual(len(result["console"]["relevantEntries"]), 1)

    def test_editor_check_allows_success_smoke_marker(self):
        responses = iter([
            {"ok": True, "result": {"status": "ready"}},
            {"ok": True, "result": {"scenePath": "Assets/Main.unity", "isDirty": False, "roots": []}},
            {"ok": True, "result": {"cursor": 10, "returned": 1,
                                      "entries": [{"level": "log", "message":
                                                   '[NOVELS_SMOKE] {"event":"catalog.ready"}'}]}},
        ])
        result = run_editor_check(self.root, self.root, False, None, "testName", 1, 0.001,
                                  sender=lambda *_: next(responses), sleeper=lambda _: None)
        self.assertTrue(result["ok"])


if __name__ == "__main__":
    unittest.main()
