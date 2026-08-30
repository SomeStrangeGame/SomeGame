import importlib.util
import json
import sys
import tempfile
import unittest
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


if __name__ == "__main__":
    unittest.main()
