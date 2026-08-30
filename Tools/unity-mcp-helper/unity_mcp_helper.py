#!/usr/bin/env python3
"""Small, dependency-free, fail-closed client for the Official Unity MCP."""

from __future__ import annotations

import argparse
import json
import os
import selectors
import signal
import socket
import statistics
import subprocess
import sys
import time
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parent
DEFAULT_MANIFEST = ROOT / "manifest.json"
MAX_LINE = 2 * 1024 * 1024


class HelperError(RuntimeError):
    def __init__(self, code: str, message: str, retryable: bool = False):
        super().__init__(message)
        self.code, self.retryable = code, retryable


def emit(value: Any, pretty: bool = False) -> None:
    print(json.dumps(value, ensure_ascii=False, indent=2 if pretty else None, separators=None if pretty else (",", ":")))


def error_payload(exc: Exception) -> dict[str, Any]:
    if isinstance(exc, HelperError):
        return {"ok": False, "error": {"code": exc.code, "message": str(exc), "retryable": exc.retryable}}
    return {"ok": False, "error": {"code": "internal_error", "message": str(exc), "retryable": False}}


class McpProcess:
    def __init__(self, command: list[str], protocol: str, startup_timeout: float = 60):
        self.command, self.protocol, self.startup_timeout = command, protocol, startup_timeout
        self.proc: subprocess.Popen[str] | None = None
        self.next_id = 1
        self.handshakes = 0

    def start(self) -> None:
        if self.proc and self.proc.poll() is None:
            return
        self.proc = subprocess.Popen(self.command, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                     stderr=subprocess.PIPE, text=True, bufsize=1)
        try:
            result = self.request("initialize", {"protocolVersion": self.protocol, "capabilities": {},
                                  "clientInfo": {"name": "unity-mcp-helper", "version": "1.0"}}, self.startup_timeout)
        except HelperError as exc:
            if exc.code == "tool_timeout":
                raise HelperError("startup_timeout", str(exc), True) from exc
            raise
        server = result.get("serverInfo", {})
        if result.get("protocolVersion") != self.protocol:
            raise HelperError("protocol_incompatible", f"Expected {self.protocol}, got {result.get('protocolVersion')}")
        if server.get("name") != "unity-mcp":
            raise HelperError("protocol_incompatible", f"Unexpected MCP server: {server.get('name')}")
        self.notify("notifications/initialized", {})
        self.handshakes += 1

    def _write(self, payload: dict[str, Any]) -> None:
        if not self.proc or not self.proc.stdin or self.proc.poll() is not None:
            raise HelperError("transport_error", "MCP process is not running", True)
        self.proc.stdin.write(json.dumps(payload, separators=(",", ":")) + "\n")
        self.proc.stdin.flush()

    def notify(self, method: str, params: dict[str, Any]) -> None:
        self._write({"jsonrpc": "2.0", "method": method, "params": params})

    def request(self, method: str, params: dict[str, Any], timeout: float) -> dict[str, Any]:
        req_id, self.next_id = self.next_id, self.next_id + 1
        self._write({"jsonrpc": "2.0", "id": req_id, "method": method, "params": params})
        assert self.proc and self.proc.stdout
        selector = selectors.DefaultSelector()
        selector.register(self.proc.stdout, selectors.EVENT_READ)
        deadline = time.monotonic() + timeout
        try:
            while time.monotonic() < deadline:
                if self.proc.poll() is not None:
                    detail = self.proc.stderr.read()[-2000:] if self.proc.stderr else ""
                    raise HelperError("transport_error", f"MCP exited ({self.proc.returncode}): {detail}", True)
                events = selector.select(max(0, deadline - time.monotonic()))
                if not events:
                    continue
                line = self.proc.stdout.readline()
                if line == "":
                    self.proc.wait(timeout=1)
                    detail = self.proc.stderr.read()[-2000:] if self.proc.stderr else ""
                    raise HelperError("transport_error", f"MCP closed stdout ({self.proc.returncode}): {detail}", True)
                if len(line) > MAX_LINE:
                    raise HelperError("invalid_response", "MCP response exceeded size limit")
                try:
                    message = json.loads(line)
                except json.JSONDecodeError as exc:
                    raise HelperError("invalid_response", f"Invalid JSON from MCP: {exc}") from exc
                if message.get("id") != req_id:
                    continue
                if "error" in message:
                    raise HelperError("unity_tool_error", json.dumps(message["error"], ensure_ascii=False))
                result = message.get("result")
                if not isinstance(result, dict):
                    raise HelperError("invalid_response", "MCP result is not an object")
                return result
            raise HelperError("tool_timeout", f"MCP request timed out after {timeout:g}s", True)
        finally:
            selector.close()

    def call(self, name: str, arguments: dict[str, Any], timeout: float) -> dict[str, Any]:
        return self.request("tools/call", {"name": name, "arguments": arguments}, timeout)

    def close(self) -> None:
        if not self.proc:
            return
        if self.proc.poll() is None:
            self.proc.terminate()
            try:
                self.proc.wait(timeout=3)
            except subprocess.TimeoutExpired:
                self.proc.kill()
                self.proc.wait(timeout=3)
        for stream in (self.proc.stdin, self.proc.stdout, self.proc.stderr):
            if stream:
                stream.close()
        self.proc = None


class Policy:
    def __init__(self, manifest_path: Path, project: Path, agent_id: str | None,
                 coordination_root: Path | None = None):
        try:
            self.data = json.loads(manifest_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            raise HelperError("configuration_error", f"Cannot load manifest: {exc}") from exc
        self.project, self.agent_id = project.resolve(), agent_id
        self.coordination_root = (coordination_root or project).resolve()

    def tool(self, name: str) -> dict[str, Any]:
        tool = self.data.get("tools", {}).get(name)
        if not tool:
            raise HelperError("tool_not_allowed", f"Tool is not allowlisted: {name}")
        return tool

    def authorize(self, name: str) -> dict[str, Any]:
        tool = self.tool(name)
        if tool.get("access") == "write":
            owner = self.coordination_root / "Docs/AI/CoordinationRuntime/active/write-lock/owner.md"
            if not self.agent_id or not owner.exists() or f"Agent: `{self.agent_id}`" not in owner.read_text(encoding="utf-8"):
                raise HelperError("lock_not_owned", "Write tool requires the caller's coordination write-lock")
        elif tool.get("access") != "read":
            raise HelperError("tool_not_allowed", f"Invalid access class for {name}")
        return tool


def decoded_content(result: dict[str, Any]) -> Any:
    content = result.get("content")
    if isinstance(content, list) and content and isinstance(content[0], dict):
        text = content[0].get("text")
        if isinstance(text, str):
            try:
                return json.loads(text)
            except json.JSONDecodeError:
                return text
    return result


def summarize(name: str, result: dict[str, Any], max_chars: int = 16000, output_format: str = "summary") -> dict[str, Any]:
    decoded = decoded_content(result)
    if output_format == "summary" and isinstance(decoded, dict):
        if name == "get_scene_hierarchy":
            decoded = {"sceneName": decoded.get("sceneName"), "scenePath": decoded.get("scenePath"),
                       "isDirty": decoded.get("isDirty"), "isActive": decoded.get("isActive"),
                       "roots": [{"name": root.get("name"), "activeSelf": root.get("activeSelf"),
                                  "components": root.get("components", [])} for root in decoded.get("roots", [])]}
        elif name == "console":
            decoded = {"entries": [{"seq": entry.get("seq"), "timestampUtc": entry.get("timestampUtc"),
                                     "level": entry.get("level"), "message": entry.get("message")}
                                    for entry in decoded.get("entries", [])],
                       "cursor": decoded.get("cursor"), "returned": decoded.get("returned"),
                       "dropped": decoded.get("dropped")}
        elif name == "list_tests":
            tests = decoded.get("Tests", decoded.get("tests", []))
            decoded = {"success": decoded.get("success"), "mode": decoded.get("Mode", decoded.get("mode")),
                       "count": decoded.get("Count", decoded.get("count")), "message": decoded.get("message"),
                       "tests": [{"fullName": test.get("FullName", test.get("fullName")),
                                  "assembly": test.get("Assembly", test.get("assembly")),
                                  "categories": test.get("Categories", test.get("categories", [])),
                                  "explicit": test.get("Explicit", test.get("explicit"))} for test in tests]}
        elif name == "test_status":
            failed = [test for test in decoded.get("results", [])
                      if str(test.get("status", "")).lower() not in {"passed", "skipped", "inconclusive"}]
            decoded = {"status": decoded.get("status"), "duration": decoded.get("duration"),
                       "summary": decoded.get("summary"),
                       "failures": [{"fullName": test.get("fullName"), "status": test.get("status"),
                                     "message": test.get("message")} for test in failed]}
    selected = decoded if output_format == "summary" else result
    raw = json.dumps(selected, ensure_ascii=False, separators=(",", ":"))
    if len(raw) <= max_chars:
        return {"ok": True, "tool": name, "result": selected, "truncated": False}
    return {"ok": True, "tool": name, "resultPreview": raw[:max_chars], "truncated": True,
            "originalChars": len(raw)}


def append_log(path: Path, event: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and path.stat().st_size > 2 * 1024 * 1024:
        path.replace(path.with_suffix(".previous.jsonl"))
    safe = {k: v for k, v in event.items() if k not in {"arguments", "result"}}
    with path.open("a", encoding="utf-8") as stream:
        stream.write(json.dumps(safe, ensure_ascii=False, separators=(",", ":")) + "\n")


def run_call(client: McpProcess, policy: Policy, name: str, arguments: dict[str, Any], log: Path,
             output_format: str = "summary", max_chars: int = 16000) -> dict[str, Any]:
    tool = policy.authorize(name)
    started = time.monotonic()
    try:
        result = client.call(name, arguments, float(tool.get("timeoutSec", 60)))
        if result.get("isError") is True:
            content = result.get("content", [])
            message = content[0].get("text") if content and isinstance(content[0], dict) else "Unity MCP tool failed"
            raise HelperError("unity_tool_error", str(message), "No Pipeline instance" in str(message))
        append_log(log, {"ts": time.time(), "tool": name, "ok": True, "durationMs": round((time.monotonic()-started)*1000)})
        return summarize(name, result, max_chars, output_format)
    except Exception as exc:
        append_log(log, {"ts": time.time(), "tool": name, "ok": False,
                         "durationMs": round((time.monotonic()-started)*1000), "error": error_payload(exc)["error"]})
        raise


def socket_path(runtime: Path) -> Path:
    return runtime / "unity-mcp-helper.sock"


def daemon(args: argparse.Namespace, policy: Policy, command: list[str]) -> int:
    runtime = Path(args.runtime).resolve(); runtime.mkdir(parents=True, exist_ok=True)
    sock_path = socket_path(runtime)
    if sock_path.exists():
        sock_path.unlink()
    server = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM); server.bind(str(sock_path)); server.listen(8)
    os.chmod(sock_path, 0o600)
    client = McpProcess(command, policy.data["protocolVersion"], args.startup_timeout)
    stopping = False
    def stop_handler(*_: Any) -> None:
        nonlocal stopping; stopping = True
    signal.signal(signal.SIGTERM, stop_handler); signal.signal(signal.SIGINT, stop_handler)
    try:
        client.start()
        while not stopping:
            server.settimeout(1)
            try: conn, _ = server.accept()
            except socket.timeout: continue
            with conn:
                line = conn.makefile("r", encoding="utf-8").readline(MAX_LINE)
                try:
                    request = json.loads(line)
                    if request.get("op") == "stop":
                        stopping = True; response = {"ok": True, "stopping": True}
                    elif request.get("op") == "status":
                        response = {"ok": True, "running": True, "pid": os.getpid(),
                                    "handshakes": client.handshakes}
                    else:
                        try:
                            response = run_call(client, policy, request["tool"], request.get("arguments", {}), Path(args.log),
                                                request.get("format", "summary"), int(request.get("maxChars", 16000)))
                        except HelperError as exc:
                            if not exc.retryable:
                                raise
                            client.close(); client.start()
                            response = run_call(client, policy, request["tool"], request.get("arguments", {}), Path(args.log),
                                                request.get("format", "summary"), int(request.get("maxChars", 16000)))
                except Exception as exc: response = error_payload(exc)
                conn.sendall((json.dumps(response, ensure_ascii=False, separators=(",", ":")) + "\n").encode())
    finally:
        client.close(); server.close()
        if sock_path.exists(): sock_path.unlink()
    return 0


def send(runtime: Path, payload: dict[str, Any], timeout: float = 130) -> dict[str, Any]:
    path = socket_path(runtime)
    if not path.exists(): raise HelperError("transport_error", "Helper daemon is not running", True)
    with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as sock:
        sock.settimeout(timeout); sock.connect(str(path)); sock.sendall((json.dumps(payload)+"\n").encode())
        line = sock.makefile("r", encoding="utf-8").readline(MAX_LINE)
    try: return json.loads(line)
    except json.JSONDecodeError as exc: raise HelperError("invalid_response", str(exc)) from exc


def percentile(values: list[float], fraction: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    rank = max(0, min(len(ordered) - 1, int((len(ordered) - 1) * fraction + 0.999999)))
    return ordered[rank]


def git_snapshot(project: Path) -> str | None:
    completed = subprocess.run(["git", "-C", str(project), "status", "--porcelain=v1", "-z"],
                               capture_output=True, text=True, check=False)
    return completed.stdout if completed.returncode == 0 else None


def result_value(response: dict[str, Any], key: str) -> Any:
    result = response.get("result")
    return result.get(key) if isinstance(result, dict) else None


def run_compile(runtime: Path, project: Path, timeout: float, poll_interval: float,
                sender: Any = send, sleeper: Any = time.sleep) -> dict[str, Any]:
    if timeout <= 0 or poll_interval <= 0:
        raise HelperError("configuration_error", "Compile timeout and poll interval must be positive")
    before_git = git_snapshot(project)
    before_console = sender(runtime, {"op": "call", "tool": "console",
                                      "arguments": {"level": "error"}, "format": "summary"})
    trigger = sender(runtime, {"op": "call", "tool": "recompile", "arguments": {},
                               "format": "summary"})
    if not trigger.get("ok"):
        error = trigger.get("error", {})
        raise HelperError(str(error.get("code", "compile_trigger_failed")),
                          str(error.get("message", "Recompile rejected")),
                          bool(error.get("retryable", False)))
    trigger_state = result_value(trigger, "status")
    polls = 0
    last_status: dict[str, Any] | None = trigger if trigger_state in {"completed", "up_to_date"} else None
    if last_status is None:
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            last_status = sender(runtime, {"op": "call", "tool": "recompile_status",
                                           "arguments": {}, "format": "summary"})
            polls += 1
            state = result_value(last_status, "status")
            if state in {"completed", "up_to_date"}:
                break
            if state in {"failed", "error", "interrupted"}:
                raise HelperError("compile_failed", json.dumps(last_status, ensure_ascii=False))
            sleeper(poll_interval)
        else:
            raise HelperError("compile_timeout", f"Recompile did not complete within {timeout:g}s", True)
    after_console = sender(runtime, {"op": "call", "tool": "console",
                                     "arguments": {"level": "error"}, "format": "summary"})
    after_git = git_snapshot(project)
    before_errors = result_value(before_console, "returned") or 0
    after_errors = result_value(after_console, "returned") or 0
    new_errors = max(0, int(after_errors) - int(before_errors))
    return {
        # Compilation is a gate, not a regression counter: errors that existed
        # before the trigger still mean the project is not compilable.
        "ok": int(after_errors) == 0,
        "trigger": trigger,
        "status": last_status,
        "polls": polls,
        "consoleErrorsBefore": before_errors,
        "consoleErrorsAfter": after_errors,
        "newConsoleErrors": new_errors,
        "gitStateComparable": before_git is not None and after_git is not None,
        "unexpectedChanges": before_git != after_git if before_git is not None and after_git is not None else None,
    }


def run_editmode_tests(runtime: Path, project: Path, timeout: float, poll_interval: float,
                       test_filter: str, filter_type: str, sender: Any = send,
                       sleeper: Any = time.sleep) -> dict[str, Any]:
    if timeout <= 0 or poll_interval <= 0:
        raise HelperError("configuration_error", "Test timeout and poll interval must be positive")
    if filter_type not in {"testName", "assembly", "category"}:
        raise HelperError("configuration_error", f"Unsupported filter type: {filter_type}")
    before_git = git_snapshot(project)
    before_console = sender(runtime, {"op": "call", "tool": "console",
                                      "arguments": {"level": "error"}, "format": "summary"})
    arguments = {"mode": "editor", "filter": test_filter, "filter_type": filter_type,
                 "include_explicit": False, "async_tests": True, "timeout": int(timeout)}
    trigger = sender(runtime, {"op": "call", "tool": "run_tests", "arguments": arguments,
                               "format": "summary"})
    if not trigger.get("ok"):
        error = trigger.get("error", {})
        if error.get("code") in {"lock_not_owned", "tool_not_allowed", "configuration_error"}:
            raise HelperError(str(error.get("code")), str(error.get("message", "Test run rejected")),
                              bool(error.get("retryable", False)))
    deadline = time.monotonic() + timeout
    polls = 0
    last_status: dict[str, Any] | None = None
    while time.monotonic() < deadline:
        last_status = sender(runtime, {"op": "call", "tool": "test_status",
                                       "arguments": {}, "format": "summary", "maxChars": 16000})
        polls += 1
        state = result_value(last_status, "status")
        if state in {"completed", "no_tests"}:
            break
        if state in {"error", "failed", "cancelled", "interrupted"}:
            raise HelperError("tests_failed", json.dumps(last_status, ensure_ascii=False))
        sleeper(poll_interval)
    else:
        raise HelperError("tests_timeout", f"EditMode tests did not complete within {timeout:g}s", True)
    after_console = sender(runtime, {"op": "call", "tool": "console",
                                     "arguments": {"level": "error"}, "format": "summary"})
    after_git = git_snapshot(project)
    summary = result_value(last_status or {}, "summary") or {}
    failed = int(summary.get("failed", 0)) if isinstance(summary, dict) else 0
    total = int(summary.get("total", 0)) if isinstance(summary, dict) else 0
    before_errors = result_value(before_console, "returned") or 0
    after_errors = result_value(after_console, "returned") or 0
    new_errors = max(0, int(after_errors) - int(before_errors))
    return {
        "ok": total > 0 and failed == 0 and new_errors == 0,
        "outcome": "no_tests" if total == 0 else ("failed" if failed > 0 or new_errors > 0 else "passed"),
        "mode": "editor",
        "filter": test_filter,
        "filterType": filter_type,
        "trigger": trigger,
        "status": last_status,
        "polls": polls,
        "total": total,
        "failed": failed,
        "noTests": total == 0,
        "consoleErrorsBefore": before_errors,
        "consoleErrorsAfter": after_errors,
        "newConsoleErrors": new_errors,
        "gitStateComparable": before_git is not None and after_git is not None,
        "unexpectedChanges": before_git != after_git if before_git is not None and after_git is not None else None,
    }


def run_editor_check(runtime: Path, project: Path, compile_scripts: bool, test_filter: str | None,
                     filter_type: str, timeout: float, poll_interval: float,
                     console_cursor: int | None = None, sender: Any = send,
                     sleeper: Any = time.sleep) -> dict[str, Any]:
    """Run one bounded Editor quality gate and return only review-relevant state."""
    deadline = time.monotonic() + timeout
    status: dict[str, Any] = {}
    while time.monotonic() < deadline:
        status = sender(runtime, {"op": "call", "tool": "editor_status", "arguments": {},
                                  "format": "summary", "maxChars": 8000})
        editor_state = status.get("result", {}) if status.get("ok") else {}
        if (editor_state.get("status") == "ready"
                and not editor_state.get("isCompiling", editor_state.get("compiling", False))
                and not editor_state.get("domainReloadInProgress", False)):
            break
        error = status.get("error", {})
        if error and not error.get("retryable", False):
            raise HelperError(str(error.get("code", "editor_not_ready")),
                              str(error.get("message", "Editor readiness failed")), False)
        sleeper(poll_interval)
    else:
        raise HelperError("editor_not_ready", f"Editor did not become ready within {timeout:g}s", True)
    hierarchy = sender(runtime, {"op": "call", "tool": "get_scene_hierarchy", "arguments": {},
                                 "format": "summary", "maxChars": 12000})
    console_arguments = {} if console_cursor is None else {"cursor": console_cursor}
    console = sender(runtime, {"op": "call", "tool": "console", "arguments": console_arguments,
                               "format": "summary", "maxChars": 16000})
    compile_result = None
    tests_result = None
    if compile_scripts:
        compile_result = run_compile(runtime, project, timeout, poll_interval, sender, sleeper)
    if test_filter is not None:
        tests_result = run_editmode_tests(runtime, project, timeout, poll_interval, test_filter,
                                          filter_type, sender, sleeper)

    editor = status.get("result", {}) if status.get("ok") else {}
    scene = hierarchy.get("result", {}) if hierarchy.get("ok") else {}
    console_result = console.get("result", {}) if console.get("ok") else {}
    entries = console_result.get("entries", []) if isinstance(console_result, dict) else []
    markers = ("INITIALIZATION_FAILED", "fallback.used", '"event":"error"', "FATAL EXCEPTION")
    relevant = [entry for entry in entries
                if str(entry.get("level", "")).lower() == "error"
                or any(marker in str(entry.get("message", "")) for marker in markers)]
    ok = all(response.get("ok") for response in (status, hierarchy, console))
    ok = ok and not relevant
    if compile_result is not None:
        ok = ok and bool(compile_result.get("ok"))
    if tests_result is not None:
        ok = ok and bool(tests_result.get("ok"))
    return {
        "ok": ok,
        "editor": {
            "status": editor.get("status"),
            "unityVersion": editor.get("unityVersion"),
            "isCompiling": editor.get("isCompiling", editor.get("compiling")),
            "domainReloadInProgress": editor.get("domainReloadInProgress"),
            "playModeState": editor.get("playModeState"),
        },
        "scene": {
            "path": scene.get("scenePath"),
            "isDirty": scene.get("isDirty"),
            "rootCount": len(scene.get("roots", [])) if isinstance(scene.get("roots"), list) else None,
        },
        "console": {
            "cursor": console_result.get("cursor") if isinstance(console_result, dict) else None,
            "returned": console_result.get("returned") if isinstance(console_result, dict) else None,
            "relevantEntries": relevant,
        },
        "compile": compile_result,
        "tests": tests_result,
    }


def run_benchmark(runtime: Path, project: Path, iterations: int) -> dict[str, Any]:
    if iterations < 1:
        raise HelperError("configuration_error", "Benchmark iterations must be at least 1")
    tools = ("editor_status", "get_scene_hierarchy", "console")
    before_git = git_snapshot(project)
    before_status = send(runtime, {"op": "status"})
    durations: list[float] = []
    failures: list[dict[str, Any]] = []
    completed_calls = 0
    started = time.monotonic()
    for iteration in range(iterations):
        for tool in tools:
            call_started = time.monotonic()
            response = send(runtime, {"op": "call", "tool": tool, "arguments": {},
                                      "format": "summary", "maxChars": 16000})
            durations.append((time.monotonic() - call_started) * 1000)
            completed_calls += 1
            if not response.get("ok"):
                failures.append({"iteration": iteration + 1, "tool": tool, "error": response.get("error")})
    samples: dict[str, dict[str, int]] = {}
    for tool in tools:
        summary = send(runtime, {"op": "call", "tool": tool, "arguments": {},
                                 "format": "summary", "maxChars": MAX_LINE})
        raw = send(runtime, {"op": "call", "tool": tool, "arguments": {},
                             "format": "json", "maxChars": MAX_LINE})
        samples[tool] = {
            "summaryChars": len(json.dumps(summary, ensure_ascii=False, separators=(",", ":"))),
            "rawChars": len(json.dumps(raw, ensure_ascii=False, separators=(",", ":"))),
        }
    after_status = send(runtime, {"op": "status"})
    after_git = git_snapshot(project)
    summary_chars = sum(sample["summaryChars"] for sample in samples.values())
    raw_chars = sum(sample["rawChars"] for sample in samples.values())
    reduction = 0.0 if raw_chars == 0 else max(0.0, 1.0 - summary_chars / raw_chars)
    successes = completed_calls - len(failures)
    return {
        "ok": not failures,
        "iterations": iterations,
        "toolsPerIteration": len(tools),
        "calls": completed_calls,
        "successes": successes,
        "failures": failures,
        "successRate": round(successes / completed_calls, 4),
        "durationMs": round((time.monotonic() - started) * 1000),
        "latencyMs": {
            "median": round(statistics.median(durations), 2),
            "p95": round(percentile(durations, 0.95), 2),
            "max": round(max(durations), 2),
        },
        "handshakes": {
            "before": before_status.get("handshakes"),
            "after": after_status.get("handshakes"),
            "duringBenchmark": (after_status.get("handshakes", 0) - before_status.get("handshakes", 0)),
        },
        "responseSamples": samples,
        "summaryChars": summary_chars,
        "rawChars": raw_chars,
        "estimatedTokenReduction": round(reduction, 4),
        "gitStateComparable": before_git is not None and after_git is not None,
        "unexpectedChanges": before_git != after_git if before_git is not None and after_git is not None else None,
    }


def parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--project", required=True); p.add_argument("--manifest", default=str(DEFAULT_MANIFEST))
    p.add_argument("--coordination-root", help="Project root containing Docs/AI/CoordinationRuntime; defaults to --project")
    p.add_argument("--unity", default="/Users/iantonishin/.unity/bin/unity")
    p.add_argument("--runtime", default="/tmp/unity-mcp-helper")
    p.add_argument("--log", default="/tmp/unity-mcp-helper/events.jsonl")
    p.add_argument("--agent-id"); p.add_argument("--startup-timeout", type=float, default=60)
    p.add_argument("--format", choices=("summary", "json"), default="summary")
    p.add_argument("--max-chars", type=int, default=16000)
    sub = p.add_subparsers(dest="command", required=True)
    sub.add_parser("serve"); sub.add_parser("status"); sub.add_parser("stop")
    benchmark = sub.add_parser("benchmark")
    benchmark.add_argument("--iterations", type=int, default=30)
    benchmark.add_argument("--report")
    compile_cmd = sub.add_parser("compile")
    compile_cmd.add_argument("--timeout", type=float, default=180)
    compile_cmd.add_argument("--poll-interval", type=float, default=1)
    tests = sub.add_parser("editmode-tests")
    tests.add_argument("--filter", default="")
    tests.add_argument("--filter-type", choices=("testName", "assembly", "category"), default="testName")
    tests.add_argument("--timeout", type=float, default=300)
    tests.add_argument("--poll-interval", type=float, default=1)
    check = sub.add_parser("editor-check")
    check.add_argument("--compile", action="store_true")
    check.add_argument("--test-filter")
    check.add_argument("--filter-type", choices=("testName", "assembly", "category"), default="testName")
    check.add_argument("--timeout", type=float, default=300)
    check.add_argument("--poll-interval", type=float, default=1)
    check.add_argument("--console-cursor", type=int)
    call = sub.add_parser("call"); call.add_argument("tool"); call.add_argument("--arguments", default="{}")
    for cmd, tool in (("editor-status", "editor_status"), ("hierarchy", "get_scene_hierarchy"),
                      ("console", "console"), ("list-tests", "list_tests")):
        q = sub.add_parser(cmd); q.set_defaults(tool=tool); q.add_argument("--arguments", default="{}")
    return p


def main() -> int:
    args = parser().parse_args(); project = Path(args.project).resolve()
    coordination_root = Path(args.coordination_root).resolve() if args.coordination_root else project
    policy = Policy(Path(args.manifest), project, args.agent_id, coordination_root)
    mcp_command = [args.unity, "mcp", "--project-path", str(project)]
    try:
        if args.command == "serve": return daemon(args, policy, mcp_command)
        if args.command == "benchmark":
            response = run_benchmark(Path(args.runtime), project, args.iterations)
            if args.report:
                report = Path(args.report).resolve()
                report.parent.mkdir(parents=True, exist_ok=True)
                report.write_text(json.dumps(response, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            emit(response, pretty=True)
            return 0 if response.get("ok") else 1
        if args.command == "compile":
            response = run_compile(Path(args.runtime), project, args.timeout, args.poll_interval)
            emit(response, pretty=True)
            return 0 if response.get("ok") else 1
        if args.command == "editmode-tests":
            response = run_editmode_tests(Path(args.runtime), project, args.timeout, args.poll_interval,
                                          args.filter, args.filter_type)
            emit(response, pretty=True)
            return 0 if response.get("ok") else 1
        if args.command == "editor-check":
            response = run_editor_check(Path(args.runtime), project, args.compile, args.test_filter,
                                        args.filter_type, args.timeout, args.poll_interval,
                                        args.console_cursor)
            emit(response, pretty=True)
            return 0 if response.get("ok") else 1
        op = "call" if hasattr(args, "tool") else args.command
        payload = {"op": op}
        if hasattr(args, "tool"):
            payload.update(tool=args.tool, arguments=json.loads(args.arguments), format=args.format, maxChars=args.max_chars)
        response = send(Path(args.runtime), payload)
        emit(response, pretty=True)
        return 0 if response.get("ok") else 1
    except Exception as exc:
        emit(error_payload(exc), pretty=True); return 1


if __name__ == "__main__":
    raise SystemExit(main())
