#!/usr/bin/env python3
"""Bounded automation workflows for the SomeGame Unity workspace."""

from __future__ import annotations

import argparse
import json
import os
import re
import signal
import subprocess
import sys
import time
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Iterable


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(Path(__file__).resolve().parent))
import task_workflows as taskflow
LOG_ROOT = ROOT / "Novels/Build/Logs/automation"
UNITY_EDITOR = Path("/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity")
MCP_CLI = Path.home() / ".unity/bin/unity"
SMOKE_PREFIX = "[NOVELS_SMOKE] "
FAILURE_TEXT = (
    "FATAL EXCEPTION", "ANR in ", "INITIALIZATION_FAILED", "catalog.load_failed",
    "Content path must remain inside", "version failure", "schema failure",
)
KNOWN_BENIGN_ANDROID_DIAGNOSTICS = (
    {
        "id": "android-emulator-sdk-controller-1970",
        "process": "qemu-system-aarch64",
        "endpoint": "127.0.0.1:1970",
        "classification": "benign_external_emulator_diagnostic",
        "affectsGate": False,
    },
)


class WorkflowError(RuntimeError):
    def __init__(self, code: str, message: str, *, details: Any = None):
        super().__init__(message)
        self.code, self.details = code, details


def utc_stamp() -> str:
    return datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")


def emit(payload: dict[str, Any]) -> None:
    print(json.dumps(payload, ensure_ascii=False, separators=(",", ":")))


def tail(path: Path, limit: int = 80) -> list[str]:
    try:
        return path.read_text(encoding="utf-8", errors="replace").splitlines()[-limit:]
    except OSError:
        return []


COMPILER_ERROR_PATTERNS = (
    re.compile(r"\berror\s+CS\d{4}\b", re.IGNORECASE),
    re.compile(r"\bScripts have compiler errors\b", re.IGNORECASE),
    re.compile(r"\bCompilation failed\b", re.IGNORECASE),
)


def compiler_error_lines(path: Path, limit: int = 40) -> list[str]:
    """Return bounded compiler failures from a log owned by this gate run."""
    try:
        lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
    except OSError:
        return []
    return [line[-1000:] for line in lines
            if any(pattern.search(line) for pattern in COMPILER_ERROR_PATTERNS)][-limit:]


def run_logged(command: list[str], *, timeout: float, log: Path,
               env: dict[str, str] | None = None) -> dict[str, Any]:
    log.parent.mkdir(parents=True, exist_ok=True)
    started = time.monotonic()
    with log.open("w", encoding="utf-8") as output:
        output.write("command=" + json.dumps(command, ensure_ascii=False) + "\n")
        output.flush()
        process = subprocess.Popen(command, cwd=ROOT, stdout=output, stderr=subprocess.STDOUT,
                                   text=True, env=env)
        deadline = time.monotonic() + timeout
        while True:
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                process.terminate()
                try:
                    process.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    process.kill(); process.wait(timeout=5)
                raise WorkflowError("timeout", f"Command timed out after {timeout:g}s",
                                    details={"command": command, "log": str(log)})
            try:
                returncode = process.wait(timeout=min(60, remaining))
                break
            except subprocess.TimeoutExpired:
                refresh_lock_heartbeat()
    return {"returncode": returncode, "durationSeconds": round(time.monotonic() - started, 3),
            "log": str(log), "tail": tail(log, 40) if returncode else []}


def lock_owner(root: Path = ROOT) -> str | None:
    owner = root / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
    if not owner.is_file():
        return None
    match = re.search(r"^- Agent:\s*`?([^`\n]+)`?", owner.read_text(encoding="utf-8"), re.M)
    return match.group(1).strip() if match else None


ACTIVE_AGENT_ID: str | None = None


def refresh_lock_heartbeat(root: Path = ROOT) -> bool:
    """Refresh only the current workflow owner's lease during a long command."""
    if not ACTIVE_AGENT_ID:
        return False
    owner = root / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
    if lock_owner(root) != ACTIVE_AGENT_ID or not owner.is_file():
        return False
    text = owner.read_text(encoding="utf-8")
    stamp = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    updated, count = re.subn(r"^- Heartbeat UTC:\s*`?[^`\n]+`?\s*$",
                             f"- Heartbeat UTC: `{stamp}`", text, count=1, flags=re.M)
    if count != 1:
        return False
    temporary = owner.with_suffix(".tmp")
    temporary.write_text(updated, encoding="utf-8")
    temporary.replace(owner)
    return True


def require_lock(agent_id: str | None) -> None:
    owner = lock_owner()
    if not agent_id:
        raise WorkflowError("agent_required", "--agent-id is required for this workflow")
    if owner != agent_id:
        raise WorkflowError("lock_not_owned", f"write-lock owner is {owner!r}, expected {agent_id!r}")


def coordination_runtime_allowlist(agent_id: str, root: Path = ROOT) -> set[str]:
    owner_path = root / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
    text = owner_path.read_text(encoding="utf-8")
    request_match = re.search(r"^- Request:\s*`?([^`\n]+)`?", text, re.M)
    if not request_match:
        raise WorkflowError("lock_request_missing", "write-lock owner has no Request field")
    request = request_match.group(1).strip()
    paths = {
        "Docs/AI/CoordinationRuntime/active/write-lock/owner.md",
        f"Docs/AI/CoordinationRuntime/requests/{request}/request.md",
        f"Docs/AI/CoordinationRuntime/agents/{agent_id}.md",
    }
    missing = [value for value in paths if not (root / value).is_file()]
    if missing:
        raise WorkflowError("coordination_record_missing", "Required coordination record is missing",
                            details=missing)
    return paths


def unexpected_git_status(lines: Iterable[str], allowed_paths: set[str]) -> list[str]:
    unexpected: list[str] = []
    for line in lines:
        if not line:
            continue
        path = line[3:]
        paths = [value.strip() for value in path.split(" -> ")]
        if line[:2] != "??" or any(value not in allowed_paths for value in paths):
            unexpected.append(line)
    return unexpected


def git_output(*parts: str, timeout: float = 30) -> str:
    try:
        result = subprocess.run(["git", *parts], cwd=ROOT, capture_output=True, text=True,
                                timeout=timeout, check=False,
                                env={**os.environ, "GIT_TERMINAL_PROMPT": "0"})
    except subprocess.TimeoutExpired as exc:
        raise WorkflowError("git_timeout", f"git {' '.join(parts)} timed out") from exc
    if result.returncode:
        raise WorkflowError("git_failed", f"git {' '.join(parts)} failed",
                            details=(result.stderr or result.stdout).strip()[-2000:])
    return result.stdout.strip()


def git_publish(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    allowed = coordination_runtime_allowlist(args.agent_id)
    branch = git_output("branch", "--show-current")
    if branch != args.branch:
        raise WorkflowError("wrong_branch", f"current branch is {branch!r}, expected {args.branch!r}")
    status_lines = git_output("status", "--porcelain=v1", "--untracked-files=all").splitlines()
    unexpected = unexpected_git_status(status_lines, allowed)
    if unexpected:
        raise WorkflowError("worktree_not_clean", "Commit or remove non-coordination changes before publishing",
                            details=unexpected[:40])

    logs: list[str] = []
    if args.ssh_key:
        key = Path(args.ssh_key).expanduser().resolve()
        if not key.is_file():
            raise WorkflowError("ssh_key_missing", f"SSH key does not exist: {key}")
        added = run_logged(["ssh-add", str(key)], timeout=min(args.timeout, 30),
                           log=LOG_ROOT / f"git-publish-{utc_stamp()}-ssh-add.log")
        logs.append(added["log"])
        if added["returncode"]:
            raise WorkflowError("ssh_add_failed", "Could not add the requested SSH key",
                                details=added["tail"])

    env = {**os.environ, "GIT_TERMINAL_PROMPT": "0"}
    fetched = run_logged(["git", "fetch", args.remote, args.branch], timeout=args.timeout,
                         log=LOG_ROOT / f"git-publish-{utc_stamp()}-fetch.log", env=env)
    logs.append(fetched["log"])
    if fetched["returncode"]:
        raise WorkflowError("fetch_failed", "git fetch failed", details=fetched["tail"])
    counts = git_output("rev-list", "--left-right", "--count",
                        f"HEAD...{args.remote}/{args.branch}").split()
    if len(counts) != 2:
        raise WorkflowError("divergence_unreadable", "Could not parse Git divergence", details=counts)
    ahead, behind = (int(value) for value in counts)
    if behind:
        raise WorkflowError("remote_ahead", "Remote contains commits missing locally; integrate them manually",
                            details={"ahead": ahead, "behind": behind})
    local_sha = git_output("rev-parse", "HEAD")
    pushed = ahead > 0
    if pushed:
        result = run_logged(["git", "push", args.remote, f"HEAD:{args.branch}"], timeout=args.timeout,
                            log=LOG_ROOT / f"git-publish-{utc_stamp()}-push.log", env=env)
        logs.append(result["log"])
        if result["returncode"]:
            raise WorkflowError("push_failed", "git push failed", details=result["tail"])
    remote_sha = git_output("ls-remote", "--heads", args.remote, f"refs/heads/{args.branch}",
                            timeout=args.timeout).split()
    remote_sha = remote_sha[0] if remote_sha else ""
    if remote_sha != local_sha:
        raise WorkflowError("remote_sha_mismatch", "Remote branch does not match local HEAD",
                            details={"local": local_sha, "remote": remote_sha})
    return {"ok": True, "workflow": "git-publish", "remote": args.remote,
            "branch": args.branch, "localSha": local_sha, "remoteSha": remote_sha,
            "aheadBefore": ahead, "pushed": pushed, "logs": logs}


def prepare_unity_lifecycle(close_hub: bool, timeout: float = 30) -> list[int]:
    processes = unity_processes()
    if processes.editors:
        raise WorkflowError("editor_running", "Batch workflow refuses to start while a Unity Editor is running",
                            details=processes.editors)
    if not processes.hubs:
        return []
    if not close_hub:
        raise WorkflowError("hub_running", "Unity Hub is running; repeat with --close-hub after preserving its state",
                            details=processes.hubs)
    pids = [item["pid"] for item in processes.hubs]
    for pid in pids: os.kill(pid, signal.SIGTERM)
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        live = {item["pid"] for item in unity_processes().hubs}
        if not live.intersection(pids): return pids
        time.sleep(0.5)
    raise WorkflowError("hub_shutdown_timeout", "Unity Hub did not exit after TERM", details=pids)


def markdown_slug(value: str) -> str:
    value = re.sub(r"[^\w\- ]", "", value.strip().lower(), flags=re.UNICODE)
    return value.replace(" ", "-")


def scan_markdown(root: Path) -> list[dict[str, str]]:
    failures: list[dict[str, str]] = []
    for source in (root / "Docs/AI").rglob("*.md"):
        text = source.read_text(encoding="utf-8", errors="replace")
        for target in re.findall(r"(?<!!)\[[^\]]*\]\(([^)]+)\)", text):
            path_text, _, anchor = target.partition("#")
            path_text = path_text.strip().strip("<>")
            if not path_text or "://" in path_text or path_text.startswith("mailto:"):
                continue
            destination = (source.parent / path_text).resolve()
            if not destination.exists():
                failures.append({"source": str(source.relative_to(root)), "target": target,
                                 "reason": "missing_target"})
                continue
            if anchor and destination.suffix.lower() == ".md":
                headings = {markdown_slug(value) for value in re.findall(
                    r"^#{1,6}\s+(.+?)\s*$", destination.read_text(encoding="utf-8", errors="replace"), re.M)}
                if anchor not in headings:
                    failures.append({"source": str(source.relative_to(root)), "target": target,
                                     "reason": "missing_anchor"})
    return failures


def docs_check(args: argparse.Namespace) -> dict[str, Any]:
    failures = scan_markdown(ROOT)
    failures.extend(taskflow.stale_context_failures(ROOT))
    limits = {
        "coordinationCore": (ROOT / "Docs/AI/rules/ParallelRefactoringCoordination.md", 140),
        "handoff": (ROOT / "Docs/AI/CoordinationRuntime/HANDOFF.md", 120),
        "memoryProject": (ROOT / "Docs/AI/memory/Project.md", 200),
        "memoryArchitecture": (ROOT / "Docs/AI/memory/Architecture.md", 200),
    }
    counts: dict[str, int] = {}
    for name, (path, maximum) in limits.items():
        count = len(path.read_text(encoding="utf-8").splitlines()); counts[name] = count
        if count > maximum:
            failures.append({"source": str(path.relative_to(ROOT)), "target": str(maximum),
                             "reason": f"line_limit_exceeded:{count}"})
    logs: list[str] = []
    for name, command in (
        ("diff-check", ["git", "diff", "--check", "--", "AGENTS.md", "Docs/AI",
                        "Tools/somegame", "Tools/somegame-tools", "Tools/novels-tools",
                        "Tools/unity-mcp-helper"]),
        ("planner-tests", [sys.executable, "-m", "unittest", "discover", "-s",
                           "Tools/novels-tools/tests", "-v"]),
        ("helper-tests", [sys.executable, "-m", "unittest", "discover", "-s",
                          "Tools/unity-mcp-helper/tests", "-v"]),
        ("automation-tests", [sys.executable, "-m", "unittest", "discover", "-s",
                              "Tools/somegame-tools/tests", "-v"]),
    ):
        result = run_logged(command, timeout=args.timeout,
                            log=LOG_ROOT / f"docs-check-{utc_stamp()}-{name}.log")
        logs.append(result["log"])
        if result["returncode"]:
            failures.append({"source": name, "target": result["log"], "reason": "command_failed"})
    return {"ok": not failures, "workflow": "docs-check", "lineCounts": counts,
            "failures": failures[:40], "failureCount": len(failures), "logs": logs}


def context_workflow(args: argparse.Namespace) -> dict[str, Any]:
    payload = taskflow.context_snapshot(ROOT, args.task, args.base_ref, args.paths, args.resume)
    payload["resume"] = args.resume
    return payload


def queue_status_workflow(args: argparse.Namespace) -> dict[str, Any]:
    state = taskflow.runtime_state(ROOT)
    process_error = None
    try:
        processes = unity_processes()
    except (OSError, RuntimeError) as exc:
        processes = UnityProcesses([], [], [])
        process_error = str(exc)
    position = next((value["position"] for value in state["requests"]
                     if value["agent"] == args.agent_id), None) if args.agent_id else None
    is_owner = bool(args.agent_id and state["lockOwner"] == args.agent_id)
    state["agentPosition"] = position
    state["isOwner"] = is_owner
    state["canAcquire"] = bool(position == 1 and state["lockOwner"] is None)
    state["canProceed"] = is_owner and state["ownerConsistent"] and not state["lockStale"]
    state["longWaitingRequests"] = [value["id"] for value in state["requests"] if value["longWaiting"]]
    state["recoverableOrphans"] = [value["id"] for value in state["requests"]
                                    if value["recoverableOrphan"]]
    state["heavyProcesses"] = ({
        "editors": processes.editors, "hubs": processes.hubs, "licensing": processes.licensing}
        if process_error is None else None)
    state["processProbe"] = "available" if process_error is None else "unavailable"
    if process_error is not None:
        state["processProbeError"] = process_error
    state["needsDeveloperDecision"] = bool(
        state["lockOwner"] and (state["lockStale"] or not state["ownerConsistent"]))
    return {"ok": True, "workflow": "queue-status", "coordination": state}


def queue_prune_workflow(args: argparse.Namespace) -> dict[str, Any]:
    state = taskflow.runtime_state(ROOT)
    request = next((value for value in state["requests"] if value["id"] == args.request), None)
    if request is None:
        raise WorkflowError("request_missing", "Exact FIFO request does not exist", details=args.request)
    if not request["recoverableOrphan"]:
        raise WorkflowError("request_not_recoverable",
                            "Request is not a terminal orphan; developer-reviewed recovery is required",
                            details=request)
    request_dir = ROOT / "Docs/AI/CoordinationRuntime/requests" / args.request
    request_file = request_dir / "request.md"
    request_file.unlink()
    request_dir.rmdir()
    return {"ok": True, "workflow": "queue-prune", "removedRequest": args.request,
            "agent": request["agent"], "agentStatus": request["agentStatus"]}


def commit_plan_workflow(args: argparse.Namespace) -> dict[str, Any]:
    return taskflow.commit_plan(ROOT)


def verification_commands(plan: dict[str, Any], args: argparse.Namespace) -> tuple[list[tuple[str, list[str]]], list[str]]:
    diff_check = ["git", "diff", "--check"]
    if args.paths is not None:
        diff_check.extend(["--", *args.paths])
    commands: list[tuple[str, list[str]]] = [("diff-check", diff_check)]
    pending: list[str] = []
    if plan["run_helper_tests"]:
        commands.append(("helper-tests", [sys.executable, "-m", "unittest", "discover", "-s",
                         "Tools/unity-mcp-helper/tests", "-v"]))
    if plan["run_automation_tests"]:
        commands.append(("automation-tests", [sys.executable, "-m", "unittest", "discover", "-s",
                         "Tools/somegame-tools/tests", "-v"]))
    for target in plan["content_targets"]:
        commands.append((f"content-{target}", [str(ROOT / "Tools/novels-tools/novels-content"),
                         "build", target, args.platform]))
    if plan["editor_compile"]:
        pending.append("editor-gate --compile")
    if plan["editmode_tests"]:
        pending.append("editor-gate --test-filter <affected-suite>")
    if plan["player_build"]:
        pending.append("player-build --target <platform> --mode <Remote|Embedded>")
    if plan["manual_visual_gate"]:
        pending.append("bounded manual visual gate")
    return commands, pending


def verify_workflow(args: argparse.Namespace) -> dict[str, Any]:
    paths = args.paths if args.paths is not None else taskflow.git_paths(ROOT, args.base_ref)
    plan = taskflow.verification_plan(ROOT, paths)
    commands, pending = verification_commands(plan, args)
    preview = {"ok": True, "workflow": "verify", "execute": not args.explain,
               "plan": plan, "commands": [value for _, value in commands], "pending": pending}
    if args.explain:
        return preview
    require_lock(args.agent_id)
    static_failures = []
    if "documentation" in plan["categories"]:
        static_failures = [*scan_markdown(ROOT), *taskflow.stale_context_failures(ROOT)]
    if static_failures:
        return {"ok": False, "complete": False, "workflow": "verify",
                "failedGate": "documentation-static", "failures": static_failures[:40]}
    fingerprint = taskflow.cache_fingerprint(ROOT, "verify", paths,
                                             {"platform": args.platform, "plan": plan})
    if not args.no_cache and not args.release:
        cached = taskflow.read_cache(ROOT, fingerprint)
        if cached:
            return {**cached, "cached": True, "fingerprint": fingerprint}
    logs: list[str] = []
    for name, command in commands:
        result = run_logged(command, timeout=args.timeout,
                            log=LOG_ROOT / f"verify-{utc_stamp()}-{name}.log")
        logs.append(result["log"])
        if result["returncode"]:
            return {"ok": False, "complete": False, "workflow": "verify", "failedGate": name,
                    "tail": result["tail"], "logs": logs, "fingerprint": fingerprint}
    payload = {"ok": True, "complete": not pending, "workflow": "verify", "cached": False,
               "fingerprint": fingerprint, "executedGates": [name for name, _ in commands],
               "pending": pending, "logs": logs}
    if payload["complete"] and not args.no_cache and not args.release:
        payload["cachePath"] = taskflow.write_cache(ROOT, fingerprint, payload)
    return payload


def finish_check(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    state = taskflow.runtime_state(ROOT); plan = taskflow.commit_plan(ROOT)
    handoff = (ROOT / "Docs/AI/CoordinationRuntime/HANDOFF.md").read_text(encoding="utf-8")
    process_probe_error: str | None = None
    try:
        processes = unity_processes()
    except (OSError, subprocess.SubprocessError) as exc:
        processes = UnityProcesses([], [], [])
        process_probe_error = str(exc)
    blockers: list[str] = []
    if args.agent_id not in handoff: blockers.append("handoff_missing_agent")
    if processes.editors: blockers.append("unity_editor_running")
    if process_probe_error: blockers.append("process_probe_unavailable")
    agent = ROOT / f"Docs/AI/CoordinationRuntime/agents/{args.agent_id}.md"
    if not agent.is_file(): blockers.append("agent_record_missing")
    elif not re.search(r"^- Status:\s*(completed|integrated)\s*$",
                       agent.read_text(encoding="utf-8"), re.M):
        blockers.append("agent_not_completed")
    return {"ok": not blockers, "workflow": "finish-check", "agentId": args.agent_id,
            "coordination": state, "blockers": blockers, "commitPlan": plan,
            "liveEditorPids": [item["pid"] for item in processes.editors],
            "processProbeError": process_probe_error}


def content_gate(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    closed_hub = prepare_unity_lifecycle(args.close_hub)
    if args.target:
        command = [str(ROOT / "Tools/novels-tools/novels-content"), "build", args.target, args.platform]
    else:
        command = [str(ROOT / "Tools/novels-tools/novels-content"), "verify", args.platform]
        if args.base_ref: command.append(args.base_ref)
    result = run_logged(command, timeout=args.timeout,
                        log=LOG_ROOT / f"content-gate-{utc_stamp()}.log")
    return {"ok": result["returncode"] == 0, "workflow": "content-gate",
            "target": args.target or "changed-path-plan", "closedHubPids": closed_hub, **result}


def default_player_output(target: str, mode: str) -> Path:
    suffix = {"Android": "Novels.apk", "iOS": "Novels", "Windows": "Novels.exe", "macOS": "Novels.app"}[target]
    return ROOT / "Novels/Build/Players/automation" / target / mode / suffix


def player_build(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    closed_hub = prepare_unity_lifecycle(args.close_hub)
    output = Path(args.output).resolve() if args.output else default_player_output(args.target, args.mode)
    logs: list[str] = []
    platform = {"Android": "android", "iOS": "ios", "Windows": "windows", "macOS": "editor"}[args.target]
    if not args.skip_content_build:
        built = run_logged([str(ROOT / "Tools/novels-tools/novels-content"), "build", "all", platform],
                           timeout=args.timeout, log=LOG_ROOT / f"player-content-{utc_stamp()}.log")
        logs.append(built["log"])
        if built["returncode"]:
            return {"ok": False, "workflow": "player-build", "stage": "content", **built}
    command = [str(ROOT / "Novels/Tools/build-player.sh"), args.mode, args.target, str(output)]
    if args.mode == "Remote": command.append(args.remote_url)
    elif args.development or args.test_signing:
        command.append("")
    if args.development: command.append("--development")
    if args.test_signing: command.append("--test-signing")
    built = run_logged(command, timeout=args.timeout, log=LOG_ROOT / f"player-{utc_stamp()}.log")
    logs.append(built["log"])
    exists = output.exists()
    return {"ok": built["returncode"] == 0 and exists, "workflow": "player-build",
            "target": args.target, "mode": args.mode, "output": str(output),
            "closedHubPids": closed_hub,
            "artifactExists": exists, "artifactBytes": output.stat().st_size if output.is_file() else None,
            "logs": logs, "tail": built["tail"]}


@dataclass
class UnityProcesses:
    editors: list[dict[str, Any]]
    hubs: list[dict[str, Any]]
    licensing: list[dict[str, Any]]


def unity_processes() -> UnityProcesses:
    result = subprocess.run(["ps", "-axo", "pid=,lstart=,command="], capture_output=True, text=True, check=True)
    editors: list[dict[str, Any]] = []; hubs: list[dict[str, Any]] = []; licensing: list[dict[str, Any]] = []
    for line in result.stdout.splitlines():
        match = re.match(r"\s*(\d+)\s+(.+?)\s+((?:/|Unity).*)$", line)
        if not match: continue
        item = {"pid": int(match.group(1)), "started": match.group(2), "command": match.group(3)}
        command = item["command"]
        if "UnityLicensingClient" in command: licensing.append(item)
        elif command.startswith("/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"): hubs.append(item)
        elif "Unity.app/Contents/MacOS/Unity" in command: editors.append(item)
    return UnityProcesses(editors, hubs, licensing)


def licensing_markers() -> tuple[list[str], list[str]]:
    log_paths = [Path.home() / "Library/Logs/Unity/Editor.log",
                 Path.home() / "Library/Logs/Unity/Unity.Licensing.Client.log"]
    patterns = ("Unsupported protocol", "Another instance", "mutex", "Connection Lost")
    matches: list[str] = []
    for path in log_paths:
        for line in tail(path, 250):
            if any(pattern.lower() in line.lower() for pattern in patterns):
                matches.append(f"{path.name}: {line[-500:]}")
    sockets = [str(path) for path in Path("/private/tmp").glob("Unity-LicenseClient-*.sock")]
    return matches[-30:], sorted(sockets)


def licensing_preflight(args: argparse.Namespace) -> dict[str, Any]:
    processes = unity_processes(); markers, sockets = licensing_markers()
    recovered: list[int] = []
    if args.recover:
        require_lock(args.agent_id)
        if processes.editors:
            raise WorkflowError("editor_running", "Recovery refuses to terminate licensing while an Editor is running",
                                details=processes.editors)
        if not markers:
            raise WorkflowError("conflict_unconfirmed", "No recent licensing conflict marker; recovery refused")
        eligible = {item["pid"]: item for item in [*processes.hubs, *processes.licensing]}
        confirmed = set(args.confirm_pid or [])
        if not confirmed:
            raise WorkflowError("pid_confirmation_required", "--recover requires one or more exact --confirm-pid values",
                                details=sorted(eligible))
        if not confirmed.issubset(eligible):
            raise WorkflowError("pid_not_eligible", "A confirmed PID is not the main Hub or Licensing Client",
                                details={"confirmed": sorted(confirmed), "eligible": sorted(eligible)})
        for item in (eligible[pid] for pid in sorted(confirmed)):
            os.kill(item["pid"], signal.SIGTERM); recovered.append(item["pid"])
        deadline = time.monotonic() + args.timeout
        while recovered and time.monotonic() < deadline:
            live = {item["pid"] for group in unity_processes().__dict__.values() for item in group}
            if not any(pid in live for pid in recovered): break
            time.sleep(0.5)
        else:
            if recovered: raise WorkflowError("recovery_timeout", "Processes did not exit after TERM", details=recovered)
        processes = unity_processes()
    conflict = bool(markers and (len(processes.licensing) > 1 or processes.hubs))
    return {"ok": not conflict and len(processes.editors) <= 1, "workflow": "licensing-preflight",
            "editors": processes.editors, "hubs": processes.hubs, "licensing": processes.licensing,
            "conflictMarkers": markers, "sockets": sockets, "terminatedPids": recovered,
            "note": "Sockets are reported but never deleted automatically."}


def wait_for(path: Path, timeout: float) -> None:
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        if path.exists(): return
        time.sleep(1)
    raise WorkflowError("startup_timeout", f"Timed out waiting for {path}")


def editor_process_options(stop_editor: bool) -> dict[str, bool]:
    return {"start_new_session": not stop_editor}


def editor_gate(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    project = (ROOT / args.project).resolve(); runtime = Path(args.runtime).resolve()
    editor: subprocess.Popen[str] | None = None; daemon: subprocess.Popen[str] | None = None
    editor_log = LOG_ROOT / f"editor-gate-{utc_stamp()}-editor.log"
    helper_log = LOG_ROOT / f"editor-gate-{utc_stamp()}-helper.log"
    helper = ROOT / "Tools/unity-mcp-helper/unity_mcp_helper.py"
    try:
        if args.start_editor:
            closed_hub = prepare_unity_lifecycle(args.close_hub)
            preflight = licensing_preflight(argparse.Namespace(recover=False, agent_id=args.agent_id, timeout=10))
            if preflight["editors"]: raise WorkflowError("editor_already_running", "--start-editor requires no live Editor")
            editor_log.parent.mkdir(parents=True, exist_ok=True)
            with editor_log.open("w", encoding="utf-8") as stream:
                editor = subprocess.Popen(
                    [str(args.unity_editor), "-projectPath", str(project),
                     "-logFile", str(editor_log)],
                    cwd=ROOT,
                    stdout=stream,
                    stderr=subprocess.STDOUT,
                    text=True,
                    **editor_process_options(args.stop_editor))
        wait_for(project / "Library/Pipeline/.unity-pipeline-port", args.startup_timeout)
        helper_log.parent.mkdir(parents=True, exist_ok=True)
        helper_stream = helper_log.open("w", encoding="utf-8")
        base = [sys.executable, str(helper), "--project", str(project), "--coordination-root", str(ROOT),
                "--agent-id", args.agent_id, "--runtime", str(runtime), "--unity", str(args.mcp_cli)]
        daemon = subprocess.Popen([*base, "serve"], cwd=ROOT, stdout=helper_stream,
                                  stderr=subprocess.STDOUT, text=True)
        wait_for(runtime / "unity-mcp-helper.sock", args.startup_timeout)
        command = [*base, "editor-check", "--timeout", str(args.timeout)]
        if args.compile: command.append("--compile")
        if args.test_filter: command += ["--test-filter", args.test_filter, "--filter-type", args.filter_type]
        result = run_logged(command, timeout=args.timeout + 30, log=LOG_ROOT / f"editor-gate-{utc_stamp()}.log")
        compiler_errors = compiler_error_lines(editor_log) if args.compile and editor else []
        ok = result["returncode"] == 0 and not compiler_errors
        return {"ok": ok, "workflow": "editor-gate",
                "project": str(project), "startedEditor": editor is not None,
                "closedHubPids": closed_hub if args.start_editor else [],
                "editorLog": str(editor_log) if editor else None, "helperLog": str(helper_log),
                "compilerErrors": compiler_errors, **result}
    finally:
        if daemon and daemon.poll() is None:
            daemon.terminate()
            try: daemon.wait(timeout=10)
            except subprocess.TimeoutExpired: daemon.kill()
        if editor and args.stop_editor and editor.poll() is None:
            editor.terminate()
            try: editor.wait(timeout=20)
            except subprocess.TimeoutExpired: editor.kill()


def parse_smoke_events(text: str) -> list[dict[str, Any]]:
    events: list[dict[str, Any]] = []
    for line in text.splitlines():
        if SMOKE_PREFIX not in line: continue
        raw = line.split(SMOKE_PREFIX, 1)[1].strip()
        json_end = raw.rfind("}")
        if json_end >= 0:
            raw = raw[:json_end + 1]
        try:
            value = json.loads(raw)
        except json.JSONDecodeError:
            continue
        if isinstance(value, dict) and all(key in value for key in ("v", "seq", "runId", "event")):
            events.append(value)
    return events


def event_sequence_ok(events: list[dict[str, Any]], required: list[str]) -> tuple[bool, list[str]]:
    if not events: return False, required
    run_id = events[-1].get("runId")
    names = [str(event.get("event")) for event in events if event.get("runId") == run_id]
    cursor = 0; missing: list[str] = []
    for expected in required:
        try: cursor = names.index(expected, cursor) + 1
        except ValueError: missing.append(expected)
    return not missing, missing


def android_log_has_blocking_marker(text: str) -> bool:
    """Match application failures only; known host-side emulator retries are non-blocking."""
    return any(marker.lower() in text.lower() for marker in FAILURE_TEXT)


def adb(args: argparse.Namespace, *parts: str, timeout: float = 60) -> subprocess.CompletedProcess[str]:
    return subprocess.run([args.adb, "-s", args.serial, *parts], capture_output=True, text=True,
                          timeout=timeout, check=False)


def android_failure_artifacts(args: argparse.Namespace, stamp: str) -> dict[str, str]:
    LOG_ROOT.mkdir(parents=True, exist_ok=True)
    paths = {"screenshot": LOG_ROOT / f"android-smoke-{stamp}.png",
             "logcat": LOG_ROOT / f"android-smoke-{stamp}-logcat.txt",
             "activity": LOG_ROOT / f"android-smoke-{stamp}-activity.txt"}
    paths["screenshot"].write_bytes(subprocess.run([args.adb, "-s", args.serial, "exec-out", "screencap", "-p"],
                                                   capture_output=True, check=False).stdout)
    paths["logcat"].write_text(adb(args, "logcat", "-d", "-v", "threadtime").stdout, encoding="utf-8")
    paths["activity"].write_text(adb(args, "shell", "dumpsys", "activity", "activities").stdout, encoding="utf-8")
    return {key: str(value) for key, value in paths.items()}


def android_smoke(args: argparse.Namespace) -> dict[str, Any]:
    require_lock(args.agent_id)
    apk = Path(args.apk).resolve()
    if not apk.is_file(): raise WorkflowError("apk_missing", f"APK does not exist: {apk}")
    stamp = utc_stamp(); required = [value for value in args.required_events.split(",") if value]
    failure: str | None = None; artifacts: dict[str, str] = {}; events: list[dict[str, Any]] = []
    device = adb(args, "get-state")
    if device.returncode or device.stdout.strip() != "device":
        raise WorkflowError("device_unavailable", device.stderr.strip() or device.stdout.strip())
    adb(args, "shell", "am", "force-stop", args.package_id)
    install = adb(args, "install", "-r", "-d", str(apk), timeout=args.install_timeout)
    if install.returncode or "Success" not in install.stdout:
        raise WorkflowError("install_failed", install.stderr.strip() or install.stdout.strip())
    adb(args, "logcat", "-c")
    launch = adb(args, "shell", "monkey", "-p", args.package_id,
                 "-c", "android.intent.category.LAUNCHER", "1")
    if launch.returncode: raise WorkflowError("launch_failed", launch.stderr.strip() or launch.stdout.strip())
    deadline = time.monotonic() + args.timeout; logcat = ""; activity = ""; pid = ""
    while time.monotonic() < deadline:
        pid = adb(args, "shell", "pidof", args.package_id).stdout.strip().replace("\r", "")
        if not pid:
            time.sleep(args.poll_interval); continue
        activity = adb(args, "shell", "dumpsys", "activity", "activities").stdout
        logcat = adb(args, "logcat", "-d", "-v", "threadtime", f"--pid={pid}").stdout
        events = parse_smoke_events(logcat)
        blocking_event = next((event for event in events if event.get("event") == "error" or
                              (event.get("event") == "fallback.used" and event.get("assetType") == "character")), None)
        if blocking_event: failure = "blocking_smoke_event"; break
        if android_log_has_blocking_marker(logcat): failure = "blocking_log_marker"; break
        sequence_ok, _ = event_sequence_ok(events, required)
        if sequence_ok and "UnityPlayerGameActivity" in activity: break
        time.sleep(args.poll_interval)
    else: failure = "smoke_timeout"
    sequence_ok, missing = event_sequence_ok(events, required)
    if not failure and (not pid or "UnityPlayerGameActivity" not in activity or not sequence_ok):
        failure = "gate_incomplete"
    if failure: artifacts = android_failure_artifacts(args, stamp)
    adb(args, "shell", "am", "force-stop", args.package_id)
    stopped = not adb(args, "shell", "pidof", args.package_id).stdout.strip()
    summary = {"ok": failure is None and stopped, "workflow": "android-smoke", "apk": str(apk),
               "apkBytes": apk.stat().st_size, "serial": args.serial, "packageId": args.package_id,
               "pid": pid, "foreground": "UnityPlayerGameActivity" in activity,
               "events": [event.get("event") for event in events], "missingEvents": missing,
               "failure": failure, "artifacts": artifacts, "forceStopped": stopped}
    summary["ignoredDiagnostics"] = list(KNOWN_BENIGN_ANDROID_DIAGNOSTICS)
    return summary


def parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(description=__doc__); sub = p.add_subparsers(dest="workflow", required=True)
    docs = sub.add_parser("docs-check"); docs.add_argument("--timeout", type=float, default=120)
    context = sub.add_parser("context"); context.add_argument("--task", choices=tuple(taskflow.TASK_ROUTES), default="code")
    context.add_argument("--resume", action="store_true"); context.add_argument("--base-ref")
    context.add_argument("--paths", nargs="*", help="Exact task-owned paths for scoped planning")
    queue_status = sub.add_parser("queue-status"); queue_status.add_argument("--agent-id")
    queue_prune = sub.add_parser("queue-prune"); queue_prune.add_argument("--request", required=True)
    verify = sub.add_parser("verify"); verify.add_argument("--agent-id")
    verify.add_argument("--base-ref"); verify.add_argument("--paths", nargs="*")
    verify.add_argument("--platform", choices=("editor", "android", "ios"), default="editor")
    verify.add_argument("--explain", action="store_true"); verify.add_argument("--no-cache", action="store_true")
    verify.add_argument("--release", action="store_true"); verify.add_argument("--timeout", type=float, default=3600)
    sub.add_parser("commit-plan")
    finish = sub.add_parser("finish-check"); finish.add_argument("--agent-id", required=True)
    publish = sub.add_parser("git-publish"); publish.add_argument("--agent-id", required=True)
    publish.add_argument("--remote", default="origin"); publish.add_argument("--branch", default="main")
    publish.add_argument("--ssh-key"); publish.add_argument("--timeout", type=float, default=120)
    content = sub.add_parser("content-gate"); content.add_argument("--agent-id", required=True)
    content.add_argument("--platform", choices=("editor", "android", "ios"), default="editor")
    content.add_argument("--target", help="Explicit catalog or story id; bypasses broad dirty-tree planning")
    content.add_argument("--close-hub", action="store_true")
    content.add_argument("--base-ref"); content.add_argument("--timeout", type=float, default=3600)
    player = sub.add_parser("player-build"); player.add_argument("--agent-id", required=True)
    player.add_argument("--target", choices=("Android", "iOS", "Windows", "macOS"), required=True)
    player.add_argument("--mode", choices=("Remote", "Embedded"), required=True)
    player.add_argument("--output"); player.add_argument("--remote-url", default="https://pureshechka.com/dev")
    signing_mode = player.add_mutually_exclusive_group()
    signing_mode.add_argument("--development", action="store_true")
    signing_mode.add_argument("--test-signing", action="store_true")
    player.add_argument("--skip-content-build", action="store_true")
    player.add_argument("--close-hub", action="store_true")
    player.add_argument("--timeout", type=float, default=7200)
    licensing = sub.add_parser("licensing-preflight"); licensing.add_argument("--agent-id")
    licensing.add_argument("--recover", action="store_true"); licensing.add_argument("--confirm-pid", type=int, action="append")
    licensing.add_argument("--timeout", type=float, default=30)
    editor = sub.add_parser("editor-gate"); editor.add_argument("--agent-id", required=True)
    editor.add_argument("--project", default="Novels"); editor.add_argument("--start-editor", action="store_true")
    editor.add_argument("--close-hub", action="store_true")
    editor.add_argument("--stop-editor", action=argparse.BooleanOptionalAction, default=True)
    editor.add_argument("--compile", action="store_true"); editor.add_argument("--test-filter")
    editor.add_argument("--filter-type", choices=("testName", "assembly", "category"), default="testName")
    editor.add_argument("--timeout", type=float, default=300); editor.add_argument("--startup-timeout", type=float, default=180)
    editor.add_argument("--runtime", default="/tmp/somegame-unity-mcp")
    editor.add_argument("--unity-editor", type=Path, default=UNITY_EDITOR); editor.add_argument("--mcp-cli", type=Path, default=MCP_CLI)
    smoke = sub.add_parser("android-smoke"); smoke.add_argument("--agent-id", required=True)
    smoke.add_argument("--apk", required=True); smoke.add_argument("--package-id", required=True)
    smoke.add_argument("--serial", default="emulator-5554"); smoke.add_argument("--adb", default="adb")
    smoke.add_argument("--timeout", type=float, default=180); smoke.add_argument("--install-timeout", type=float, default=180)
    smoke.add_argument("--poll-interval", type=float, default=2)
    smoke.add_argument("--required-events", default="app.started,catalog.loading,catalog.ready,story.selected,release.activated,episode.selected,episode.ready,dialogue.ready")
    return p


def main() -> int:
    args = parser().parse_args(); started = time.monotonic()
    global ACTIVE_AGENT_ID
    ACTIVE_AGENT_ID = getattr(args, "agent_id", None)
    handlers = {"docs-check": docs_check, "context": context_workflow, "verify": verify_workflow,
                "queue-status": queue_status_workflow,
                "queue-prune": queue_prune_workflow,
                "commit-plan": commit_plan_workflow, "finish-check": finish_check,
                "git-publish": git_publish,
                "content-gate": content_gate, "player-build": player_build,
                "licensing-preflight": licensing_preflight, "editor-gate": editor_gate,
                "android-smoke": android_smoke}
    try:
        payload = handlers[args.workflow](args); payload["durationSeconds"] = round(time.monotonic() - started, 3)
        emit(payload); return 0 if payload.get("ok") else 1
    except Exception as exc:
        if isinstance(exc, WorkflowError):
            payload = {"ok": False, "workflow": args.workflow, "error": {"code": exc.code,
                       "message": str(exc), "details": exc.details}}
        else:
            payload = {"ok": False, "workflow": args.workflow,
                       "error": {"code": "internal_error", "message": str(exc)}}
        payload["durationSeconds"] = round(time.monotonic() - started, 3); emit(payload); return 1


if __name__ == "__main__":
    raise SystemExit(main())
