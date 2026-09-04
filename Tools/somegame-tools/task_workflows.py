#!/usr/bin/env python3
"""Compact context, verification, commit and finish planning for SomeGame."""

from __future__ import annotations

import hashlib
import json
import os
import re
import subprocess
from dataclasses import asdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


TASK_ROUTES = {
    "inspect": [],
    "docs": ["Docs/AI/guides/AutomationRunners.md"],
    "code": ["Docs/AI/rules/ParallelWorkDetails.md"],
    "unity": ["Docs/AI/memory/Workflows.md", "Docs/AI/guides/UnityMcpWorkflow.md"],
    "content": ["Docs/AI/memory/Workflows.md", "Docs/AI/guides/ContentPipeline.md"],
    "art": ["Docs/AI/rules/CharacterLayeringRules.md", "Docs/AI/guides/ManualContentChecklist.md"],
    "integration": ["Docs/AI/rules/IntegrationProtocol.md", "Docs/AI/memory/Workflows.md"],
}
BASE_DOCUMENTS = [
    "Docs/AI/rules/ParallelRefactoringCoordination.md",
    "Docs/AI/memory/Project.md",
    "Docs/AI/memory/Architecture.md",
]
STATUS_RE = re.compile(r"^- (?:Статус|Status):\s*`?([^`\n]+)", re.M)
FIELD_RE = re.compile(r"^- ([^:\n]+):\s*`?([^`\n]+)", re.M)
STALE_SECONDS = 10 * 60
TERMINAL_REQUEST_STATUSES = {
    "completed", "cancelled", "canceled", "paused", "yielded",
    "ready-for-integration", "ready-with-limitations",
}


def command(root: Path, *parts: str) -> str:
    result = subprocess.run(parts, cwd=root, capture_output=True, text=True, check=False)
    if result.returncode:
        raise RuntimeError((result.stderr or result.stdout).strip() or "command failed")
    return result.stdout.strip()


def git_paths(root: Path, base: str | None = None) -> list[str]:
    values: set[str] = set()
    commands = []
    if base:
        commands.append(("git", "diff", "--name-only", f"{base}...HEAD"))
    commands.extend((("git", "diff", "--name-only"), ("git", "diff", "--cached", "--name-only"),
                     ("git", "ls-files", "--others", "--exclude-standard")))
    for parts in commands:
        values.update(value for value in command(root, *parts).splitlines() if value)
    return sorted(values)


def verification_plan(root: Path, paths: list[str]) -> dict[str, Any]:
    import importlib.util
    import sys
    module_path = root / "Tools/novels-tools/verification.py"
    spec = importlib.util.spec_from_file_location("somegame_verification", module_path)
    if not spec or not spec.loader:
        raise RuntimeError("verification planner cannot be loaded")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return asdict(module.classify(paths, module.catalog_story_ids(root)))


def record_fields(path: Path) -> dict[str, str]:
    if not path.is_file():
        return {}
    return {key.strip(): value.strip().rstrip(".")
            for key, value in FIELD_RE.findall(path.read_text(encoding="utf-8", errors="replace"))}


def utc_age_seconds(value: str | None, now: datetime | None = None) -> int | None:
    if not value:
        return None
    try:
        parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError:
        return None
    current = now or datetime.now(timezone.utc)
    return max(0, int((current - parsed).total_seconds()))


def runtime_state(root: Path, now: datetime | None = None) -> dict[str, Any]:
    runtime = root / "Docs/AI/CoordinationRuntime"
    owner = runtime / "active/write-lock/owner.md"
    owner_fields = record_fields(owner)
    request_paths = sorted((runtime / "requests").glob("*/request.md"))
    requests = []
    for position, path in enumerate(request_paths, 1):
        fields = record_fields(path)
        agent = fields.get("Agent")
        agent_fields = record_fields(runtime / "agents" / f"{agent}.md") if agent else {}
        requested = fields.get("Requested UTC")
        age = utc_age_seconds(requested, now)
        agent_status = agent_fields.get("Status")
        requests.append({"id": path.parent.name, "agent": agent, "position": position,
                         "status": fields.get("Status"), "agentStatus": agent_fields.get("Status"),
                         "ageSeconds": age, "longWaiting": (age or 0) > STALE_SECONDS,
                         "recoverableOrphan": agent_status in TERMINAL_REQUEST_STATUSES})
    heartbeat_age = utc_age_seconds(owner_fields.get("Heartbeat UTC"), now)
    owner_request = owner_fields.get("Request")
    for request in requests:
        request["longWaiting"] = bool(
            request["longWaiting"] and request["id"] != owner_request)
        request["recoverableOrphan"] = bool(
            request["recoverableOrphan"] and request["id"] != owner_request)
    first_request = requests[0]["id"] if requests else None
    owner_consistent = not owner_fields or (owner_request == first_request and
                       any(value["id"] == owner_request and value["agent"] == owner_fields.get("Agent")
                           for value in requests))
    if owner_fields:
        blocked_reason = "active_lock" if owner_consistent else "inconsistent_lock"
    elif requests:
        blocked_reason = "first_request_must_acquire"
    else:
        blocked_reason = None
    return {"lockOwner": owner_fields.get("Agent"), "lockRequest": owner_request,
            "heartbeatAgeSeconds": heartbeat_age,
            "lockStale": heartbeat_age is None or heartbeat_age > STALE_SECONDS if owner_fields else False,
            "ownerConsistent": owner_consistent, "firstRequest": first_request,
            "requestCount": len(requests), "requests": requests, "blockedReason": blocked_reason}


def work_state(root: Path) -> list[dict[str, str]]:
    records: list[dict[str, str]] = []
    for path in sorted((root / "Docs/AI/work/parallel").glob("ParallelWork.*.md")):
        if path.name == "ParallelWork.queue.md":
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        match = STATUS_RE.search(text)
        status = match.group(1).strip() if match else "unknown"
        if status not in {"integrated", "completed"}:
            records.append({"id": path.stem.removeprefix("ParallelWork."), "status": status})
    return records


def open_handoff(root: Path) -> list[dict[str, str]]:
    text = (root / "Docs/AI/CoordinationRuntime/HANDOFF.md").read_text(encoding="utf-8")
    active = text.split("## Runtime rules", 1)[0]
    values: list[str] = []
    current: list[str] = []
    for line in active.splitlines():
        if line.startswith("- "):
            if current: values.append(" ".join(current))
            current = [line[2:].strip()]
        elif current and line.startswith("  "):
            current.append(line.strip())
        elif current and not line.strip():
            values.append(" ".join(current)); current = []
    if current: values.append(" ".join(current))
    values = [re.sub(r"\s+", " ", value).strip() for value in values]
    return [{"summary": value[:200]} for value in values[:12]]


def context_snapshot(root: Path, task: str, base: str | None = None,
                     owned_paths: list[str] | None = None, resume: bool = False) -> dict[str, Any]:
    dirty_paths = git_paths(root, base)
    plan_paths = owned_paths if owned_paths is not None else dirty_paths
    plan = verification_plan(root, plan_paths)
    documents = list(dict.fromkeys([*BASE_DOCUMENTS, *TASK_ROUTES[task]]))
    next_command = "Tools/somegame verify --explain"
    if task == "integration":
        next_command = "Tools/somegame commit-plan"
    document_fingerprints = {
        value: hashlib.sha256((root / value).read_bytes()).hexdigest()[:12]
        for value in documents
    }
    return {"ok": True, "workflow": "context", "task": task,
            "git": {"branch": command(root, "git", "branch", "--show-current"),
                    "head": command(root, "git", "rev-parse", "--short=12", "HEAD"),
                    "dirtyCount": len(dirty_paths), "dirtyPaths": dirty_paths[:20],
                    "pathsTruncated": len(dirty_paths) > 20},
            "coordination": runtime_state(root), "openWork": work_state(root)[:20],
            "openHandoff": open_handoff(root), "documents": documents,
            "documentFingerprints": document_fingerprints,
            "documentMode": "reuse-if-unchanged" if resume else "read-full",
            "planningBasis": "owned-paths" if owned_paths is not None else "working-tree",
            "planPaths": plan_paths,
            "plan": {key: plan[key] for key in ("categories", "content_targets", "static_only",
                     "run_helper_tests", "run_automation_tests", "editor_compile", "editmode_tests",
                     "player_build", "manual_visual_gate")}, "nextCommand": next_command}


def commit_plan(root: Path) -> dict[str, Any]:
    groups: dict[str, list[str]] = {}
    excluded: list[str] = []
    changed = git_paths(root)
    for path in changed:
        if any(part in path.split("/") for part in ("Library", "Temp", "Logs")) or path.endswith(".DS_Store"):
            excluded.append(path); continue
        if path.startswith("Docs/AI/CoordinationRuntime/"): group = "runtime-handoff"
        elif path.startswith("Docs/AI/") or path == "AGENTS.md": group = "protocol-documentation"
        elif path.startswith("Tools/"): group = "tooling"
        elif path.startswith("Packages/"): group = "shared-packages"
        elif path.startswith("Novels/"): group = "game-runtime"
        elif path.startswith("Projects/"): group = path.split("/", 2)[1]
        else: group = "repository-config"
        groups.setdefault(group, []).append(path)
    order = ["tooling", "protocol-documentation", "shared-packages", "game-runtime"]
    names = sorted(groups, key=lambda value: (order.index(value) if value in order else len(order), value))
    active_owners: list[dict[str, Any]] = []
    for record in sorted((root / "Docs/AI/CoordinationRuntime/agents").glob("*.md")):
        text = record.read_text(encoding="utf-8", errors="replace")
        status = STATUS_RE.search(text)
        if status and status.group(1).strip() == "active":
            conflicts = [path for path in changed if f"`{path}`" in text]
            active_owners.append({"agent": record.stem, "exactPathConflicts": conflicts})
    return {"ok": True, "workflow": "commit-plan",
            "groups": [{"name": name, "paths": groups[name]} for name in names],
            "excluded": excluded, "activeOwners": active_owners,
            "note": "Read-only recommendation; review semantic dependencies before staging."}


def cache_fingerprint(root: Path, workflow: str, paths: list[str], options: dict[str, Any]) -> str:
    digest = hashlib.sha256()
    digest.update(b"somegame-validation-cache-v1\0")
    digest.update(workflow.encode()); digest.update(json.dumps(options, sort_keys=True).encode())
    fixed = ["Tools/somegame-tools/runner.py", "Tools/somegame-tools/task_workflows.py",
             "Tools/novels-tools/verification.py"]
    projects = [root / "Novels", *sorted((root / "Projects").glob("novels-*"))]
    for project in projects:
        for suffix in ("ProjectSettings/ProjectVersion.txt", "Packages/manifest.json",
                       "Packages/packages-lock.json"):
            fixed.append(str((project / suffix).relative_to(root)))
    for value in sorted(set([*paths, *fixed])):
        path = root / value
        digest.update(value.encode()); digest.update(b"\0")
        if path.is_file(): digest.update(path.read_bytes())
    return digest.hexdigest()


def cache_path(root: Path, fingerprint: str) -> Path:
    return root / "Library/SomeGameValidationCache" / f"{fingerprint}.json"


def read_cache(root: Path, fingerprint: str) -> dict[str, Any] | None:
    path = cache_path(root, fingerprint)
    try: value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError): return None
    return value if value.get("ok") and value.get("complete") else None


def write_cache(root: Path, fingerprint: str, payload: dict[str, Any]) -> str:
    path = cache_path(root, fingerprint); path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, ensure_ascii=False, separators=(",", ":")), encoding="utf-8")
    return str(path)


def stale_context_failures(root: Path) -> list[dict[str, str]]:
    failures: list[dict[str, str]] = []
    handoff = root / "Docs/AI/CoordinationRuntime/HANDOFF.md"
    lines = len(handoff.read_text(encoding="utf-8").splitlines())
    if lines > 120:
        failures.append({"source": str(handoff.relative_to(root)), "target": "120",
                         "reason": f"handoff_rotation_due:{lines}"})
    valid = {"active", "blocked", "paused", "ready-for-integration", "ready-with-limitations", "yielded"}
    for path in sorted((root / "Docs/AI/work/parallel").glob("ParallelWork.*.md")):
        if path.name == "ParallelWork.queue.md": continue
        match = STATUS_RE.search(path.read_text(encoding="utf-8", errors="replace"))
        status = match.group(1).strip() if match else "unknown"
        if status not in valid:
            failures.append({"source": str(path.relative_to(root)), "target": status,
                             "reason": "inactive_or_unknown_work_record"})
    return failures
