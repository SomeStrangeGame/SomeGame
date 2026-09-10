# Agent: `kostroma-rebrand`

- Status: ready-for-release-build
- Task: Rebrand unreleased Nochelessie app to Kostroma everywhere current and codify Russian-city prerelease naming in release skill
- Scope: Projects/apps/nochelessie/**;Projects/apps/kostroma/**;Website/**;.agents/skills/somegame-release-app/**;Docs/AI/guides/ContentPipeline.md;Docs/AI/guides/AutomationRunners.md;Docs/AI/plans/SlavicMysticismMvp.md;Projects/novels-chernaya-melnitsa/Art/NARRATIVE_PACKAGE.md;Projects/novels-chernaya-melnitsa/README.md;Projects/novels-kolodets-kotoryy-zovet/Art/NARRATIVE_PACKAGE.md;Projects/novels-volchya-poshlina/Assets/Ink/s01e01.ink;Projects/novels-volchya-poshlina/README.md;Tools/somegame-tools/README.md;Tools/somegame-tools/tests/test_runner.py;remote:/home/p/pureshecom/public_html/index.html;remote:/home/p/pureshecom/public_html/site/releases/**;remote:/home/p/pureshecom/public_html/content/kostroma-dev.json;remote:/home/p/pureshecom/public_html/DevBuilds/Kostroma-dev.apk
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T12:19:39Z`.

- Progress: current app profile, package IDs, website source, active story/product references, tooling examples/tests and release skill now use Kostroma. Website build and 40 runner tests pass; scoped diff checks pass. Skill quick validator is unavailable because the bundled Python environments lack PyYAML, but frontmatter was preserved and inspected.
- Pending: fresh user-authorized Android Remote dev content/Player build, public `kostroma-dev.json`, `Kostroma-dev.apk`, site switch and HTTP/package verification.
