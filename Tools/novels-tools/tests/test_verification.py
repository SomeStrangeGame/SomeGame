import importlib.util
import sys
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "verification.py"
SPEC = importlib.util.spec_from_file_location("novels_verification", MODULE_PATH)
assert SPEC and SPEC.loader
MODULE = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = MODULE
SPEC.loader.exec_module(MODULE)
classify = MODULE.classify


class VerificationPlannerTests(unittest.TestCase):
    def test_docs_are_static_only(self):
        plan = classify(["Docs/AI/guides/ContentPipeline.md"], ["tzm", "zdm"])
        self.assertTrue(plan.static_only)
        self.assertFalse(plan.editor_compile)
        self.assertEqual(plan.content_targets, [])

    def test_repository_metadata_is_static_only(self):
        plan = classify([".gitignore"], ["tzm", "zdm"])
        self.assertTrue(plan.static_only)
        self.assertEqual(plan.categories, ["repository-config"])

    def test_story_change_selects_only_that_story(self):
        plan = classify(["Projects/novels-tzm/Assets/Ink/tzm.ink"], ["tzm", "zdm"])
        self.assertEqual(plan.content_targets, ["tzm"])
        self.assertFalse(plan.player_build)

    def test_shared_sdk_selects_release_set(self):
        plan = classify(["Packages/NovelsContentSdk/Runtime/Foo.cs"], ["tzm", "zdm"])
        self.assertEqual(plan.content_targets, ["catalog", "tzm", "zdm"])
        self.assertTrue(plan.editor_compile)
        self.assertTrue(plan.editmode_tests)

    def test_player_settings_require_player_gate(self):
        plan = classify(["Novels/ProjectSettings/ProjectSettings.asset"], ["tzm"])
        self.assertTrue(plan.editor_compile)
        self.assertTrue(plan.player_build)

    def test_helper_change_selects_unit_tests_without_unity(self):
        plan = classify(["Tools/unity-mcp-helper/unity_mcp_helper.py"], ["tzm"])
        self.assertTrue(plan.run_helper_tests)
        self.assertFalse(plan.editor_compile)

    def test_somegame_runner_change_selects_automation_tests_without_unity(self):
        plan = classify(["Tools/somegame-tools/runner.py"], ["tzm", "zdm"])
        self.assertTrue(plan.run_automation_tests)
        self.assertFalse(plan.run_helper_tests)
        self.assertFalse(plan.editor_compile)


if __name__ == "__main__":
    unittest.main()
