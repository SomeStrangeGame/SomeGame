import importlib.util
import json
import sys
import tempfile
import unittest
from datetime import datetime, timezone
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "task_workflows.py"
SPEC = importlib.util.spec_from_file_location("task_workflows_test", MODULE_PATH)
module = importlib.util.module_from_spec(SPEC)
assert SPEC and SPEC.loader
sys.modules[SPEC.name] = module
SPEC.loader.exec_module(module)


class TaskWorkflowTests(unittest.TestCase):
    def test_routes_are_small_and_reference_existing_documents(self):
        root = MODULE_PATH.parents[2]
        for documents in module.TASK_ROUTES.values():
            self.assertLessEqual(len(documents), 3)
            self.assertTrue(all((root / value).is_file() for value in documents))

    def test_commit_plan_groups_tooling_and_runtime_separately(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            module.git_paths = lambda _root: ["Tools/a.py", "Docs/AI/a.md",
                                               "Docs/AI/CoordinationRuntime/HANDOFF.md"]
            result = module.commit_plan(root)
            self.assertEqual(["tooling", "protocol-documentation", "runtime-handoff"],
                             [value["name"] for value in result["groups"]])

    def test_stale_context_detects_large_handoff_and_integrated_work(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); runtime = root / "Docs/AI/CoordinationRuntime"
            work = root / "Docs/AI/work/parallel"; runtime.mkdir(parents=True); work.mkdir(parents=True)
            (runtime / "HANDOFF.md").write_text("\n".join("line" for _ in range(121)), encoding="utf-8")
            (work / "ParallelWork.done.md").write_text("- Статус: integrated\n", encoding="utf-8")
            reasons = [value["reason"] for value in module.stale_context_failures(root)]
            self.assertTrue(any(value.startswith("handoff_rotation_due") for value in reasons))
            self.assertIn("inactive_or_unknown_work_record", reasons)

    def test_validation_cache_roundtrip_requires_complete_success(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); payload = {"ok": True, "complete": True, "workflow": "verify"}
            module.write_cache(root, "abc", payload)
            self.assertEqual("verify", module.read_cache(root, "abc")["workflow"])
            (module.cache_path(root, "abc")).write_text(json.dumps({"ok": True, "complete": False}))
            self.assertIsNone(module.read_cache(root, "abc"))

    def test_context_routes_fit_compact_budget(self):
        self.assertLessEqual(sum(len(value) for value in module.TASK_ROUTES.values()), 12)

    def test_inspect_route_adds_no_topic_documents(self):
        self.assertEqual([], module.TASK_ROUTES["inspect"])

    def test_context_uses_owned_paths_and_resume_fingerprints(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for value in module.BASE_DOCUMENTS:
                path = root / value; path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(value, encoding="utf-8")
            runtime = root / "Docs/AI/CoordinationRuntime"
            runtime.mkdir(parents=True)
            (runtime / "HANDOFF.md").write_text("", encoding="utf-8")
            (root / "Docs/AI/work/parallel").mkdir(parents=True)
            module.verification_plan = lambda _root, paths: {
                "categories": paths, "content_targets": [], "static_only": True,
                "run_helper_tests": False, "run_automation_tests": False,
                "editor_compile": False, "editmode_tests": False,
                "player_build": False, "manual_visual_gate": False,
            }
            module.git_paths = lambda _root, base=None: ["foreign.md"]
            module.command = lambda _root, *parts: "branch" if "branch" in parts else "abc"
            result = module.context_snapshot(root, "inspect", owned_paths=["owned.md"], resume=True)
            self.assertEqual("owned-paths", result["planningBasis"])
            self.assertEqual("reuse-if-unchanged", result["documentMode"])
            self.assertEqual(["owned.md"], result["plan"]["categories"])
            self.assertEqual(["owned.md"], result["planPaths"])
            self.assertEqual(["foreign.md"], result["git"]["dirtyPaths"])
            self.assertEqual(set(module.BASE_DOCUMENTS), set(result["documentFingerprints"]))

    def test_runtime_state_reports_stale_inconsistent_owner_and_positions(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); runtime = root / "Docs/AI/CoordinationRuntime"
            lock = runtime / "active/write-lock"; requests = runtime / "requests"
            agents = runtime / "agents"; lock.mkdir(parents=True); agents.mkdir(parents=True)
            for stamp, agent in (("20260101T000000Z", "first"), ("20260101T000100Z", "second")):
                path = requests / f"{stamp}-{agent}"; path.mkdir(parents=True)
                (path / "request.md").write_text(
                    f"- Agent: `{agent}`\n- Status: queued\n- Requested UTC: `2026-01-01T00:00:00Z`\n")
                (agents / f"{agent}.md").write_text("- Status: queued\n")
            (lock / "owner.md").write_text(
                "- Agent: `second`\n- Request: `20260101T000100Z-second`\n"
                "- Heartbeat UTC: `2026-01-01T00:00:00Z`\n")
            state = module.runtime_state(root, datetime(2026, 1, 1, 0, 11, tzinfo=timezone.utc))
            self.assertTrue(state["lockStale"])
            self.assertFalse(state["ownerConsistent"])
            self.assertEqual("inconsistent_lock", state["blockedReason"])
            self.assertEqual([1, 2], [value["position"] for value in state["requests"]])
            self.assertEqual([True, False], [value["longWaiting"] for value in state["requests"]])
            self.assertEqual([False, False], [value["recoverableOrphan"] for value in state["requests"]])


if __name__ == "__main__":
    unittest.main()
