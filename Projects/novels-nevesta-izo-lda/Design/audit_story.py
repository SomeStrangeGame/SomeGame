"""Read-only audit of this story's restricted source Ink; NOT an Ink compiler.

Run with Python 3. Unsupported syntax fails closed. Enumerates source choices,
assignments, conditionals and diverts, and checks exact asset paths/aliases.
Compiled Ink, Unity import and Player acceptance remain separate gates.
"""

import ast
import hashlib
import json
from pathlib import Path
import re
import unicodedata

if not __debug__:
    raise SystemExit('Run without -O: this audit relies on assertions.')


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Assets/Ink/s01e01.ink'
TEXT = SOURCE.read_text()
LINES = [line.strip() for line in TEXT.splitlines()]


def expression(text, state):
    node = ast.parse(text.replace('&&', ' and ').replace('||', ' or '), mode='eval')

    def visit(n):
        if isinstance(n, ast.Expression):
            return visit(n.body)
        if isinstance(n, ast.Constant) and type(n.value) in (int, bool):
            return n.value
        if isinstance(n, ast.Name):
            return {'true': True, 'false': False, **state}[n.id]
        if isinstance(n, ast.UnaryOp) and isinstance(n.op, ast.Not):
            return not visit(n.operand)
        if isinstance(n, ast.BoolOp):
            values = [visit(v) for v in n.values]
            if isinstance(n.op, ast.And):
                return all(values)
            if isinstance(n.op, ast.Or):
                return any(values)
        if isinstance(n, ast.Compare) and len(n.ops) == 1:
            a, b = visit(n.left), visit(n.comparators[0])
            op = n.ops[0]
            if isinstance(op, ast.GtE):
                return a >= b
            if isinstance(op, ast.Gt):
                return a > b
            if isinstance(op, ast.Lt):
                return a < b
            if isinstance(op, ast.Eq):
                return a == b
        raise AssertionError(f'Unsupported expression: {text}')

    return visit(node)


def parse_block(index, stops=()):
    nodes = []
    while index < len(LINES):
        line = LINES[index]
        if line in stops:
            return nodes, index
        if not line or line.startswith('//'):
            index += 1
            continue
        if line.startswith('{'):
            assert line.endswith(':'), (index + 1, line)
            yes, boundary = parse_block(index + 1, ('- else:', '}'))
            no = []
            if LINES[boundary] == '- else:':
                no, boundary = parse_block(boundary + 1, ('}',))
            assert LINES[boundary] == '}'
            nodes.append(('if', line[1:-1].strip(), yes, no))
            index = boundary + 1
            continue
        assert line not in ('}', '- else:'), (index + 1, line)
        nodes.append(('line', index + 1, line))
        index += 1
    assert not stops, 'Unclosed conditional'
    return nodes, index


KNOTS = {}
for i, line in enumerate(LINES):
    match = re.fullmatch(r'===\s*(\w+)\s*===', line)
    if match:
        assert match[1] not in KNOTS, 'Duplicate knot'
        KNOTS[match[1]] = i
DIVERTS = re.findall(r'->\s*(\w+)', TEXT)
assert set(DIVERTS) <= set(KNOTS) | {'END'}, 'Undefined divert'
LABELS = re.findall(r'^\*\s*\((\w+)\)', TEXT, re.M)
assert len(LABELS) == len(set(LABELS)), 'Duplicate choice label'
STATE = {}
for name, value in re.findall(r'^VAR (\w+) = (.+)$', TEXT, re.M):
    STATE[name] = expression(value, STATE)
BLOCKS = {}
ordered = list(KNOTS.items())
for k, (name, index) in enumerate(ordered):
    end = ordered[k + 1][1] if k + 1 < len(ordered) else len(LINES)
    # Parse one knot at a time, using a local temporary view of source lines.
    original = LINES[:]
    LINES = LINES[:end]
    BLOCKS[name], _ = parse_block(index + 1)
    LINES = original

ASSETS = {}
for folder, prefix in [('Characters', 'story/character/characters'),
                       ('Locations', 'story/location/locations'),
                       ('Choices', 'story/choose/items')]:
    for path in (ROOT / 'Assets' / folder).rglob('*.png'):
        key = f'{prefix}/{path.relative_to(ROOT / "Assets" / folder).as_posix()}'
        key = unicodedata.normalize('NFC', key).lower()
        assert key not in ASSETS, 'Canonical asset collision'
        ASSETS[key] = path
definition = (ROOT / 'Assets/nevesta-izo-lda.asset').read_text()
ALIASES = dict(re.findall(r'_alias: (\S+)\s+_target: (\S+)', definition))
MAIN = re.search(r'_mainCharacter: "([^"]+)"', definition)[1]
USED = set()


def asset(address):
    address = unicodedata.normalize('NFC', address).lower()
    seen = set()
    while address in ALIASES:
        assert address not in seen, 'Alias cycle'
        seen.add(address)
        address = ALIASES[address]
    assert address in ASSETS, f'Missing runtime address: {address}'
    USED.add(ASSETS[address])


def inspect_line(line):
    if line.startswith('Локация:'):
        asset('story/location/locations/' + line.split(':', 1)[1].strip() + '.png')
        return 0
    if re.match(r'(Музыка|Звук|Звуки окружения):', line):
        key = line.split(':', 1)[1].strip()
        hits = [p for p in (ROOT / 'Assets/Audio').glob(key + '.*')
                if p.suffix.lower() in ('.wav', '.ogg', '.mp3')]
        assert len(hits) == 1, f'Missing/ambiguous audio: {key}'
        USED.add(hits[0])
        return 0
    if line.startswith(('Название:', 'Серия:', 'Жанры:', 'Аннотация:', 'Статы:',
                        'Уведомление:')):
        return 0
    speaker = re.fullmatch(r'(.+?) \(([^)]+)\): (.+)', line)
    if speaker:
        name = 'maincharacter' if speaker[1] == MAIN else speaker[1]
        args = [p.strip() for p in speaker[2].split(',')]
        assert len(args) >= 2
        assert set(args[2:]) <= {'невидимка', 'убрать невидимку'}
        asset(f'story/character/characters/{name}/view/whole/{args[0]}/main.png')
        asset(f'story/character/characters/{name}/view/whole/{args[0]}/{args[1]}.png')
        value = speaker[3]
    else:
        assert line.startswith('...: '), f'Unsupported content: {line}'
        value = line[5:]
    assert not any(x in value for x in ('{', '}', '[', ']')), 'Unsupported inline Ink'
    if value == 'КОНЕЦ СЕРИИ':
        return 0
    return len(re.findall(r'[А-Яа-яЁёA-Za-z0-9]+(?:[-’][А-Яа-яЁёA-Za-z0-9]+)*', value))


RESULTS = []
VISITED = set()
BRANCHES = set()
FINAL_MENUS = {}


def walk(nodes, state, words=0, choices=(), path=(), markers=0, rendered=()):
    if not nodes:
        raise AssertionError(f'Route falls through: {path}, {choices}')
    node, rest = nodes[0], nodes[1:]
    if node[0] == 'if':
        value = bool(expression(node[1], state))
        BRANCHES.add((node[1], value))
        return walk((node[2] if value else node[3]) + rest,
                    state, words, choices, path, markers, rendered)
    _, number, line = node
    VISITED.add(number)
    if line.startswith('* '):
        # The authored subset has top-level choices, each with an explicit divert.
        starts = [i for i, n in enumerate(nodes)
                  if n[0] == 'line' and n[2].startswith('* ')]
        assert starts[0] == 0
        available = []
        for j, index in enumerate(starts):
            choice = nodes[index][2]
            match = re.fullmatch(r'\* \((\w+)\) (?:\{([^{}]+)\} )?\[(.+) # choice_icon:([\w-]+)\]', choice)
            assert match, f'Unsupported choice syntax: {choice}'
            if match[2]:
                condition = match[2].strip()
                allowed = bool(expression(condition, state))
                BRANCHES.add(('choice: ' + condition, allowed))
                if not allowed:
                    continue
            asset('story/choose/items/' + match[4] + '.png')
            finish = starts[j + 1] if j + 1 < len(starts) else len(nodes)
            available.append((match[1], nodes[index + 1:finish]))
        assert available, 'No available choice'
        if len(choices) == 4:
            rescue, boundary, _, crossing = choices
            eligible = boundary == 'untie_boundary' and (
                rescue == 'take_hand' or crossing == 'follow_leda')
            expected_menu = (('shared_promise',) if eligible else ()) + ('return_leda', 'offer_memory')
            menu = tuple(label for label, _ in available)
            assert menu == expected_menu, ('Final menu contract drift', choices, menu)
            FINAL_MENUS[choices] = menu
        for label, body in available:
            walk(body, dict(state), words, choices + (label,), path, markers, rendered)
        return
    if line.startswith('~ '):
        match = re.fullmatch(r'~ (\w+) (\+=|-=|=) (.+)', line)
        assert match and match[1] in state, line
        name, op, value = match.groups()
        val = expression(value, state)
        state[name] = val if op == '=' else state[name] + (val if op == '+=' else -val)
    elif line.startswith('-> '):
        dest = line[3:].strip()
        if dest == 'END':
            assert markers == 1, 'Missing or duplicate episode end marker'
            assert len(choices) == 5
            RESULTS.append({'ending': path[-1], 'words': words,
                            'choices': choices, 'state': dict(state), 'lines': rendered})
            return
        assert dest not in path, f'Unexpected route cycle: {dest}'
        return walk(BLOCKS[dest], state, words, choices, path + (dest,), markers, rendered)
    else:
        words += inspect_line(line)
        markers += line == '...: КОНЕЦ СЕРИИ'
        rendered += (line,)
    walk(rest, state, words, choices, path, markers, rendered)


entry = re.search(r'^-> (\w+)', TEXT, re.M)[1]
walk(BLOCKS[entry], dict(STATE), path=(entry,))
assert len(RESULTS) == 38
assert {r['ending'] for r in RESULTS} == {'ending_thaw', 'ending_glass', 'ending_lantern'}
assert len({r['choices'] for r in RESULTS}) == 38, 'Duplicate route'
assert len(FINAL_MENUS) == 16
# Independent authored contract: derive expected state from the choices, not
# from the source conditions/assignments interpreted above.
for route in RESULTS:
    rescue, boundary, confession, crossing, payment = route['choices']
    assert rescue in ('take_hand', 'call_witnesses')
    assert boundary in ('untie_boundary', 'cut_boundary')
    assert confession in ('hear_osip', 'demand_ledger')
    assert crossing in ('follow_leda', 'follow_savva')
    assert payment in ('shared_promise', 'return_leda', 'offer_memory')
    expected = {
        'trust_leda': sum((rescue == 'take_hand', boundary == 'untie_boundary',
                           crossing == 'follow_leda')),
        'trust_village': sum((rescue == 'call_witnesses', confession == 'hear_osip',
                              crossing == 'follow_savva')) - (boundary == 'cut_boundary'),
        'truth_osip': confession == 'hear_osip',
        'ribbon_free': boundary == 'untie_boundary',
        'promise_shared': payment == 'shared_promise',
        'memory_offered': payment == 'offer_memory',
    }
    assert route['state'] == expected, ('State contract drift', route['choices'])
    ending = 'ending_glass'
    if payment == 'offer_memory':
        ending = 'ending_lantern'
    elif (payment == 'shared_promise' and expected['trust_leda'] >= 2
          and expected['ribbon_free']):
        ending = 'ending_thaw'
    assert route['ending'] == ending, ('Ending contract drift', route['choices'])
assert all({value for expr, value in BRANCHES if expr == condition} == {False, True}
           for condition, _ in BRANCHES), 'Unexercised conditional outcome'
assert all(3000 <= r['words'] <= 5000 for r in RESULTS), (
    'Route length outside contract', min(r['words'] for r in RESULTS),
    max(r['words'] for r in RESULTS))
production = set(ASSETS.values()) | set((ROOT / 'Assets/Audio').glob('*.wav'))
assert USED == production, f'Unused assets: {production - USED}'
summary = {}
for ending in sorted({r['ending'] for r in RESULTS}):
    routes = [r for r in RESULTS if r['ending'] == ending]
    summary[ending] = {'routes': len(routes),
                       'minWords': min(r['words'] for r in routes),
                       'maxWords': max(r['words'] for r in routes),
                       'exampleChoices': routes[0]['choices']}
print(json.dumps({'sourceSha256': hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
                  'mode': 'restricted-source-analysis-not-compiled-Ink',
                  'routes': summary, 'assetsResolved': len(USED),
                  'finalMenus': {'preFinalStates': len(FINAL_MENUS),
                                 'withSharedPromise': sum('shared_promise' in m for m in FINAL_MENUS.values())},
                  'stateRanges': {name: [min(r['state'][name] for r in RESULTS),
                                         max(r['state'][name] for r in RESULTS)]
                                  for name in STATE},
                  'conditionsExercised': sorted(BRANCHES)}, ensure_ascii=False, indent=2))
