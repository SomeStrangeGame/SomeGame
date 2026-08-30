#!/usr/bin/env python3
"""Changed-path validation planner for the SomeGame Unity workspace."""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from dataclasses import asdict, dataclass, field
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


@dataclass
class VerificationPlan:
    paths: list[str]
    path_count: int = 0
    paths_truncated: bool = False
    categories: list[str] = field(default_factory=list)
    content_targets: list[str] = field(default_factory=list)
    static_only: bool = False
    run_helper_tests: bool = False
    run_automation_tests: bool = False
    editor_compile: bool = False
    editmode_tests: bool = False
    player_build: bool = False
    manual_visual_gate: bool = False
    reasons: list[str] = field(default_factory=list)


def catalog_story_ids(root: Path = ROOT) -> list[str]:
    config = root / "Projects/novels-catalog/Config/catalog.json"
    try:
        data = json.loads(config.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return []
    stories = data.get("stories", [])
    return [value for value in stories if isinstance(value, str) and value]


def changed_paths(root: Path = ROOT, base: str | None = None) -> list[str]:
    commands = []
    if base:
        commands.append(["git", "-c", "core.quotepath=false", "-C", str(root),
                         "diff", "--name-only", f"{base}...HEAD"])
    commands.extend([
        ["git", "-c", "core.quotepath=false", "-C", str(root), "diff", "--name-only"],
        ["git", "-c", "core.quotepath=false", "-C", str(root), "diff", "--cached", "--name-only"],
        ["git", "-c", "core.quotepath=false", "-C", str(root), "ls-files", "--others", "--exclude-standard"],
    ])
    values: set[str] = set()
    for command in commands:
        result = subprocess.run(command, capture_output=True, text=True, check=False)
        if result.returncode != 0:
            raise RuntimeError(result.stderr.strip() or "git path discovery failed")
        values.update(line for line in result.stdout.splitlines() if line)
    return sorted(values)


def classify(paths: list[str], release_stories: list[str] | None = None) -> VerificationPlan:
    all_paths = sorted(set(paths))
    plan = VerificationPlan(paths=all_paths[:40], path_count=len(all_paths), paths_truncated=len(all_paths) > 40)
    release_targets = ["catalog", *(release_stories or [])]
    content: set[str] = set()
    categories: set[str] = set()
    docs_only = bool(all_paths)

    for path in all_paths:
        if path.startswith("Docs/AI/") or path in {"AGENTS.md", "README.md"}:
            categories.add("documentation")
            continue
        if path in {".gitignore", ".gitattributes", ".editorconfig"}:
            categories.add("repository-config")
            continue
        docs_only = False
        if (path.startswith("Tools/unity-mcp-helper/") or path.startswith("Tools/novels-tools/") or
                path == "Tools/somegame" or path.startswith("Tools/somegame-tools/")):
            categories.add("tooling")
            plan.run_helper_tests |= path.startswith("Tools/unity-mcp-helper/")
            plan.run_automation_tests |= path == "Tools/somegame" or path.startswith("Tools/somegame-tools/")
            continue
        if path.startswith("Packages/"):
            categories.add("shared-sdk")
            content.update(release_targets)
            plan.editor_compile = True
            plan.editmode_tests = True
            continue
        if path.startswith("Novels/Assets/"):
            categories.add("game-runtime")
            plan.editor_compile = True
            plan.editmode_tests = True
            if any(token in path for token in ("/UI/", ".prefab", ".unity")):
                plan.manual_visual_gate = True
            continue
        if path.startswith("Novels/ProjectSettings/") or path.startswith("Novels/Assets/Settings/Build Profiles/"):
            categories.add("player-settings")
            plan.editor_compile = True
            plan.player_build = True
            continue
        if path.startswith("Projects/novels-"):
            parts = path.split("/", 2)
            project = parts[1] if len(parts) > 1 else ""
            target = project.removeprefix("novels-")
            if target:
                categories.add("atomic-content")
                content.add(target)
                if "/Assets/" in path and any(token in path.lower() for token in (".png", ".jpg", ".jpeg", ".webp", ".prefab")):
                    plan.manual_visual_gate = True
            continue
        if "/Packages/manifest.json" in path or "/Packages/packages-lock.json" in path:
            categories.add("unity-packages")
            plan.editor_compile = True
            continue
        categories.add("other")

    plan.categories = sorted(categories)
    plan.content_targets = [target for target in release_targets if target in content]
    plan.content_targets.extend(sorted(content - set(plan.content_targets)))
    plan.static_only = docs_only or not all_paths
    if plan.static_only:
        plan.reasons.append("Only documentation/coordination changed; Unity is not required.")
    if plan.content_targets:
        plan.reasons.append("Build only affected production content targets for the selected platform.")
    if plan.editor_compile:
        plan.reasons.append("Use one aggregated persistent Editor check instead of repeated status polling.")
    if plan.player_build:
        plan.reasons.append("Player/platform settings changed; schedule one target Player build after Editor checks.")
    if plan.manual_visual_gate:
        plan.reasons.append("Visual assets or UI changed; keep a bounded manual visual gate.")
    return plan


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base")
    parser.add_argument("--paths", nargs="*")
    parser.add_argument("--compact", action="store_true")
    args = parser.parse_args()
    try:
        paths = args.paths if args.paths is not None else changed_paths(ROOT, args.base)
        plan = classify(paths, catalog_story_ids(ROOT))
        print(json.dumps(asdict(plan), ensure_ascii=False, indent=None if args.compact else 2,
                         separators=(",", ":") if args.compact else None))
        return 0
    except Exception as exc:
        print(json.dumps({"ok": False, "error": str(exc)}, ensure_ascii=False), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
