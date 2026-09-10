#!/usr/bin/env python3
"""Build a fail-closed SomeGame channel manifest."""

from __future__ import annotations

import argparse
import json
import re
from collections import OrderedDict
from pathlib import Path


SELECTOR_RE = re.compile(r"^(?P<story>[a-z0-9_-]+)=(?P<version>[a-z0-9._-]+)$")


def parse_selectors(values: list[str]) -> OrderedDict[str, str]:
    updates: OrderedDict[str, str] = OrderedDict()
    for value in values:
        match = SELECTOR_RE.fullmatch(value)
        if match is None:
            raise ValueError(f"Invalid story version selector: {value}")
        story = match.group("story")
        if story in updates:
            raise ValueError(f"Duplicate story: {story}")
        updates[story] = match.group("version")
    if not updates:
        raise ValueError("At least one story-id=version is required.")
    return updates


def load_manifest(path: Path) -> OrderedDict[str, str]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"), object_pairs_hook=OrderedDict)
    except (OSError, json.JSONDecodeError) as error:
        raise ValueError(f"Cannot read base manifest '{path}': {error}") from error
    if not isinstance(value, dict) or value.get("schema") != 1:
        raise ValueError("Base manifest must have schema 1.")
    stories = value.get("stories")
    if not isinstance(stories, dict):
        raise ValueError("Base manifest stories must be an object.")
    validated: OrderedDict[str, str] = OrderedDict()
    for story, version in stories.items():
        selector = f"{story}={version}"
        parsed = parse_selectors([selector])
        validated[story] = parsed[story]
    return validated


def compose(
    updates: OrderedDict[str, str],
    base: OrderedDict[str, str] | None,
) -> tuple[OrderedDict[str, str], dict[str, object]]:
    stories = OrderedDict(base or ())
    before = OrderedDict(stories)
    for story, version in updates.items():
        stories[story] = version
    added = [story for story in stories if story not in before]
    updated = [story for story in stories if story in before and stories[story] != before[story]]
    retained = [story for story in stories if story in before and stories[story] == before[story]]
    return stories, {
        "ok": True,
        "mode": "merge" if base is not None else "replace",
        "comparisonAvailable": base is not None,
        "retained": retained,
        "added": added,
        "updated": updated,
        "removed": [] if base is not None else None,
        "storyCount": len(stories),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--base-manifest", type=Path)
    mode.add_argument("--replace", action="store_true")
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("selectors", nargs="+")
    args = parser.parse_args()

    try:
        updates = parse_selectors(args.selectors)
        base = load_manifest(args.base_manifest) if args.base_manifest else None
        stories, summary = compose(updates, base)
    except ValueError as error:
        parser.error(str(error))

    args.output.parent.mkdir(parents=True, exist_ok=True)
    document = OrderedDict((("schema", 1), ("stories", stories)))
    args.output.write_text(
        json.dumps(document, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(json.dumps(summary, ensure_ascii=False, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
