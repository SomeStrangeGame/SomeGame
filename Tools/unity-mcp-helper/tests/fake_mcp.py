#!/usr/bin/env python3
import json, sys, time

for line in sys.stdin:
    request = json.loads(line)
    if "id" not in request:
        continue
    method = request.get("method")
    if method == "initialize":
        result = {"protocolVersion": "2025-06-18", "capabilities": {"tools": {}},
                  "serverInfo": {"name": "unity-mcp", "version": "fake"}}
    elif method == "tools/call":
        name = request["params"]["name"]
        if name == "slow": time.sleep(2)
        if name == "malformed":
            print("not-json", flush=True)
            continue
        if name == "crash":
            raise SystemExit(7)
        result = {"content": [{"type": "text", "text": name}], "isError": False}
    else:
        result = {}
    print(json.dumps({"jsonrpc": "2.0", "id": request["id"], "result": result}), flush=True)
