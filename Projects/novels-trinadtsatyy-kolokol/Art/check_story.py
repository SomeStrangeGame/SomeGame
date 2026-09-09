"""Read-only checks for this story's limited Ink syntax; NOT an Ink compiler.

Run with python3 Art/check_story.py from the story project (or any directory).
Add --self-test to check that in-memory regressions are correctly rejected.
Fails on unsupported control syntax, enumerates actual source choices, and checks
speaker presence plus the current SDK's maincharacter/main.png address contract.
"""
import ast
import copy
import hashlib
import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INK = ROOT / "Assets/Ink"
STORY = "trinadtsatyy-kolokol"


def expression(source, state):
    tree = ast.parse(source.replace("&&", " and ").replace("||", " or ").strip(), mode="eval")

    def visit(node):
        if isinstance(node, ast.Expression):
            return visit(node.body)
        if isinstance(node, ast.Constant) and isinstance(node.value, (int, bool)):
            return node.value
        if isinstance(node, ast.Name):
            if node.id in ("true", "false"):
                return node.id == "true"
            return state["vars"][node.id]
        if isinstance(node, ast.Attribute) and isinstance(node.value, ast.Name):
            return int(f"{node.value.id}.{node.attr}" in state["visits"])
        if isinstance(node, ast.UnaryOp) and isinstance(node.op, ast.Not):
            return not visit(node.operand)
        if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Add):
            return visit(node.left) + visit(node.right)
        if isinstance(node, ast.BoolOp):
            values = [visit(v) for v in node.values]
            if isinstance(node.op, ast.And):
                return all(values)
            if isinstance(node.op, ast.Or):
                return any(values)
        if isinstance(node, ast.Compare) and len(node.ops) == 1:
            left, right = visit(node.left), visit(node.comparators[0])
            op = node.ops[0]
            if isinstance(op, ast.GtE):
                return left >= right
            if isinstance(op, ast.Gt):
                return left > right
            if isinstance(op, ast.Lt):
                return left < right
        raise AssertionError(f"Unsupported expression: {source}")

    return visit(tree)


class Parser:
    def __init__(self, lines):
        self.lines, self.index = lines, 0

    def sequence(self, choice_body=False):
        result = []
        while self.index < len(self.lines):
            src, line, text = self.lines[self.index]
            if text in ("}", "- else:") or (choice_body and (text == "-" or text.startswith("* "))):
                break
            self.index += 1
            if text.startswith("{") and text.endswith(":"):
                yes = self.sequence()
                no = []
                if self.index < len(self.lines) and self.lines[self.index][2] == "- else:":
                    self.index += 1
                    no = self.sequence()
                assert self.index < len(self.lines) and self.lines[self.index][2] == "}", (src, line, "unclosed condition")
                self.index += 1
                result.append(("if", text[1:-1].strip(), yes, no))
            elif text.startswith("* "):
                self.index -= 1
                options = []
                while self.index < len(self.lines) and self.lines[self.index][2].startswith("* "):
                    option = self.lines[self.index]
                    match = re.fullmatch(r"\* \((\w+)\)\s*(?:\{([^{}]+)\}\s*)?\[([^\[\]]+)\]", option[2])
                    assert match, (option, "unsupported choice")
                    self.index += 1
                    options.append((match[1], match[2] or "true", match[3], self.sequence(True)))
                if self.index < len(self.lines) and self.lines[self.index][2] == "-":
                    self.index += 1
                result.append(("choice", options))
            elif text.startswith("VAR ") or text.startswith("~ "):
                target, value = text.split(" ", 1)[1].split("=", 1)
                assert re.fullmatch(r"\w+", target.strip()), text
                result.append(("set", target.strip(), value.strip()))
            elif text.startswith("-> "):
                target = text[3:].strip()
                assert re.fullmatch(r"\w+", target), text
                result.append(("divert", target))
            else:
                assert ":" in text and not text.startswith(("{", "-", "+", "~", "*")), (src, line, text)
                result.append(("text", src, line, text))
        return result


def main():
    card = json.loads((ROOT / "Config/card.json").read_text())
    assert card["storyId"] == STORY and (ROOT / "Config" / card["cover"]).is_file()
    root_source = (INK / f"{STORY}.ink").read_text()
    includes = re.findall(r"^INCLUDE (\S+\.ink)$", root_source, re.M)
    assert includes == [f"s01e{i:02}.ink" for i in range(1, 7)]
    sources = {name: (INK / name).read_text() for name in includes}
    sections, section = {"entry": []}, "entry"
    for name, source in sources.items():
        for number, raw in enumerate(source.splitlines(), 1):
            text = raw.strip()
            if not text or text.startswith("//"):
                continue
            knot = re.fullmatch(r"=== (\w+) ===", text)
            if knot:
                section = knot[1]
                assert section not in sections, section
                sections[section] = []
            else:
                sections[section].append((name, number, text))
    nodes = {}
    for name, lines in sections.items():
        parser = Parser(lines)
        nodes[name] = parser.sequence()
        assert parser.index == len(lines), (name, "unparsed source")

    definition = (ROOT / f"Assets/{STORY}.asset").read_text()
    assert re.findall(r"  - _id: (s\d+e\d+)", definition) == [f"s01e{i:02}" for i in range(1, 7)]
    main_character = re.search(r'_mainCharacter: "([^"]+)"', definition)[1]
    defaults = dict(re.findall(r'_character: "([^"]+)"\s+_clothes: (\S+)', definition))
    paths = {name: ROOT / "Assets/Characters" / ("maincharacter" if name == main_character else name.lower()) / "view/whole" / outfit for name, outfit in defaults.items()}
    for path in paths.values():
        assert (path / "main.png").is_file(), f"Missing neutral runtime address: {path}/main.png"

    locations, audio, appearances, covered, routes = set(), set(), set(), set(), []

    def walk(todo, state, knot, steps=0):
        assert steps < 3000, "Unexpected cycle"
        if not todo:
            raise AssertionError(f"Nonterminal dead end: {knot}")
        node, rest = todo[0], todo[1:]
        kind = node[0]
        if kind == "divert":
            if node[1] == "END":
                assert knot.startswith("ending_"), knot
                assert state["episodes"] == [f"TKs01e{i:02}" for i in range(1, 7)]
                routes.append((knot, state))
                return
            assert node[1] in nodes, node
            knot = node[1]
            if knot.startswith("TKs"):
                state["episodes"].append(knot)
            return walk(nodes[knot], state, knot, steps + 1)
        if kind == "set":
            state["vars"][node[1]] = expression(node[2], state)
        elif kind == "if":
            branch = node[2] if expression(node[1], state) else node[3]
            return walk(branch + rest, state, knot, steps + 1)
        elif kind == "choice":
            available = [o for o in node[1] if expression(o[1], state)]
            assert len(available) >= 2, (knot, "no meaningful choice")
            if any(o[0] == "publish_archive" for o in node[1]):
                expected = {"destroy_circuit", "take_the_watch"}
                if state["vars"]["evidence"] >= 8:
                    expected.add("publish_archive")
                assert {o[0] for o in available} == expected, (
                    "Final availability must depend on evidence only", state["vars"])
            for label, _, caption, branch in available:
                fork = copy.deepcopy(state)
                fork["visits"].add(f"{knot}.{label}")
                fork["choices"].append(label)
                covered.add(label)
                fork["words"] += len(caption.split())
                walk(branch + rest, fork, knot, steps + 1)
            return
        elif kind == "text":
            _, source, number, text = node
            speaker, body = text.split(":", 1)
            speaker, body = speaker.strip(), body.strip()
            if speaker == "Локация":
                assert (ROOT / "Assets/Locations" / (body + ".png")).is_file(), (source, number, body)
                locations.add(body)
            elif speaker in ("Звук", "Музыка"):
                assert len([p for p in (ROOT / "Assets/Audio").glob(body + ".*") if p.suffix in (".wav", ".mp3", ".ogg")]) == 1, (source, number, body)
                audio.add(body)
            else:
                state["words"] += len(body.split())
                if speaker == "...":
                    pass
                else:
                    match = re.fullmatch(r"([^()]+?)(?: \((\w+)\))?", speaker)
                    assert match and match[1] in defaults, (source, number, speaker)
                    name, variant = match[1], match[2]
                    if variant:
                        state["appearance"][name] = variant
                    variant = state["appearance"].get(name, "main")
                    assert (paths[name] / (variant + ".png")).is_file(), (source, number, name, variant)
                    appearances.add((name, variant))
                    if source == "s01e06.ink":
                        assert name != "Инга" or state["vars"]["mercy_inga"], (number, "absent Inga speaks")
                        assert name != "Тим" or state["vars"]["saved_tim"], (number, "missing Tim speaks")
        else:
            raise AssertionError(node)
        walk(rest, state, knot, steps + 1)

    initial = {"vars": {}, "visits": set(), "choices": [], "appearance": {}, "words": 0, "episodes": []}
    walk(nodes["entry"], initial, "entry")
    assert len(covered) == 13, covered
    assert len({tuple(s["choices"][:-1]) for _, s in routes}) == 32
    for ending, state in routes:
        last = state["choices"][-1]
        assert ending == {"publish_archive": "ending_city_hears", "destroy_circuit": "ending_quiet_shift", "take_the_watch": "ending_zero_watch"}[last], (last, ending, "wrong ending")
        if last == "publish_archive":
            assert state["vars"]["evidence"] >= 8
        if ending == "ending_zero_watch":
            assert state["vars"]["saved_tim"], "watch must record Tim's return"
        assert len(state["choices"]) == 6
    published = [s for e, s in routes if e == "ending_city_hears"]
    assert {s["vars"]["saved_tim"] for s in published} == {True, False}
    assert {s["vars"]["mercy_inga"] for s in published} == {True, False}
    assert locations == {p.stem for p in (ROOT / "Assets/Locations").glob("*.png")}
    assert audio == {p.stem for p in (ROOT / "Assets/Audio").glob("*.wav")}
    print(json.dumps({
        "result": "passed", "scope": "source subset, not compiled Ink or Unity",
        "source_sha256": {n: hashlib.sha256(s.encode()).hexdigest() for n, s in sources.items()},
        "pre_final_routes": 32, "complete_routes": len(routes),
        "ending_routes": dict(Counter(e for e, _ in routes)),
        "publication_tim_saved": sum(s["vars"]["saved_tim"] for s in published),
        "publication_tim_missing": sum(not s["vars"]["saved_tim"] for s in published),
        "publication_inga_present": sum(s["vars"]["mercy_inga"] for s in published),
        "publication_inga_absent": sum(not s["vars"]["mercy_inga"] for s in published),
        "visible_words_per_route": [min(s["words"] for _, s in routes), max(s["words"] for _, s in routes)],
        "evidence_range": [min(s["vars"]["evidence"] for _, s in routes), max(s["vars"]["evidence"] for _, s in routes)],
        "location_ids": len(locations), "audio_ids": len(audio), "character_addresses": len(appearances),
        "covered_options": sorted(covered),
    }, ensure_ascii=False, indent=2))


def self_test():
    """Mutate reads in memory only; never write fixture or story files."""
    from contextlib import redirect_stdout
    from io import StringIO
    from unittest.mock import patch

    guard = "* (publish_archive) { evidence >= 8 }"
    cases = [
        ("Tim-dependent publication", guard, guard[:-2] + " && saved_tim }", "Final availability"),
        ("Inga-dependent publication", guard, guard[:-2] + " && mercy_inga }", "Final availability"),
        ("publication without evidence", guard, "* (publish_archive)", "Final availability"),
        ("missing Tim speaks", "{ saved_tim:\n    Тим (main): Слышишь? Ничего.",
         "{ true:\n    Тим (main): Слышишь? Ничего.", "missing Tim speaks"),
        ("absent Inga speaks", "{ mercy_inga:\n    Инга (ashamed): Усилители",
         "{ true:\n    Инга (ashamed): Усилители", "absent Inga speaks"),
        ("wrong final divert", "    -> ending_city_hears", "    -> ending_quiet_shift", "wrong ending"),
        ("watch fails to record Tim's return",
         "...: Последним вернулся Тим. Он слышал сестру внутри каждой водосточной трубы.\n    ~ saved_tim = true",
         "...: Последним вернулся Тим. Он слышал сестру внутри каждой водосточной трубы.",
         "watch must record Tim's return"),
    ]
    original_read = Path.read_text
    for label, old, new, expected in cases:
        def read(path, *args, **kwargs):
            source = original_read(path, *args, **kwargs)
            if path.name == "s01e06.ink":
                if source.count(old) != 1:
                    raise RuntimeError(f"Stale regression fixture: {label}")
                return source.replace(old, new, 1)
            return source

        with patch.object(Path, "read_text", read), redirect_stdout(StringIO()):
            try:
                main()
            except AssertionError as error:
                assert expected in str(error), (label, "unexpected rejection", str(error))
            else:
                raise AssertionError(f"Regression was not detected: {label}")
        print(f"Regression rejected: {label}")


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()
    main()
    if args.self_test:
        self_test()
