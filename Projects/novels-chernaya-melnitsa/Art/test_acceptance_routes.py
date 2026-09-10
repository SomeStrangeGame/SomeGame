import unittest
from plan_acceptance_routes import build_plan


class AcceptanceRoutePlanTests(unittest.TestCase):
    def test_complete_authored_branch_cover_without_runtime_claim(self):
        plan = build_plan()
        self.assertEqual(plan['staticCombinations'], 72)
        self.assertEqual(plan['coverage']['choice'], 12)
        self.assertEqual(plan['coverage']['ending'], 3)
        self.assertEqual(plan['uncovered'], [])
        self.assertTrue(all(r['runtimeStatus'] == 'not-run' and r['runtimeEvidence'] == []
                            for r in plan['routes']))
        self.assertTrue(all(len(r['choices']) == 5 for r in plan['routes']))
        self.assertEqual({r['ending'] for r in plan['routes']}, {'road','silence','keeper'})

    def test_reproducible(self):
        self.assertEqual(build_plan(), build_plan())


if __name__ == '__main__':
    unittest.main()
