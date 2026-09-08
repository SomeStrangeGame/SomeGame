"""Read-only checks for this story's small Ink subset; NOT the Ink compiler.

Enumerates choices and conditional/divert paths, checks runtime selectors and
reports conservative displayed-word counts. Unsupported syntax fails closed.
Run from any directory with Python 3; no dependencies or generated files.
"""
import ast
import copy
import hashlib
import json
from pathlib import Path
import re
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Assets/Ink/s01e01.ink'
TEXT = SOURCE.read_text()
LINES = list(enumerate(TEXT.splitlines(), 1))


def expression(source, state):
    tree = ast.parse(source.replace('||', ' or ').replace('&&', ' and '), mode='eval')

    def visit(node):
        if isinstance(node, ast.Constant):
            return node.value
        if isinstance(node, ast.Name):
            if node.id in ('true', 'false'):
                return node.id == 'true'
            return state[node.id]
        if isinstance(node, ast.Compare) and len(node.ops) == 1 and isinstance(node.ops[0], ast.Eq):
            return visit(node.left) == visit(node.comparators[0])
        if isinstance(node, ast.BoolOp) and isinstance(node.op, (ast.Or, ast.And)):
            values = [bool(visit(x)) for x in node.values]
            return any(values) if isinstance(node.op, ast.Or) else all(values)
        raise AssertionError(f'Unsupported expression: {source}')

    return visit(tree.body)


class Parser:
    def __init__(self, lines):
        self.lines, self.i = lines, 0

    def sequence(self, stops=()):
        nodes = []
        while self.i < len(self.lines):
            number, raw = self.lines[self.i]
            line = raw.strip()
            if any(test(line) for test in stops):
                break
            self.i += 1
            if not line or line.startswith('//') or line == '-':
                continue
            if line.startswith('{'):
                assert line.endswith(':'), (number, line)
                branches, condition = [], line[1:-1].strip()
                while True:
                    body = self.sequence((lambda s: s == '}' or s.startswith('- '),))
                    branches.append((condition, body))
                    assert self.i < len(self.lines), f'Unclosed condition at {number}'
                    delimiter = self.lines[self.i][1].strip()
                    self.i += 1
                    if delimiter == '}':
                        break
                    assert delimiter.startswith('- ') and delimiter.endswith(':'), delimiter
                    assert delimiter == '- else:' and len(branches) == 1, (
                        'Inline-start Ink condition permits only one else; nest additional conditions',
                        self.lines[self.i - 1][0], delimiter)
                    condition = delimiter[2:-1].strip()
                nodes.append(('if', number, branches))
            elif line.startswith('* '):
                options = []
                while True:
                    match = re.fullmatch(r'\* \((\w+)\) \[(.+)\]', line)
                    assert match, (number, line)
                    body = self.sequence((lambda s: s.startswith('* ') or s == '-',))
                    options.append((match[1], match[2], body))
                    assert self.i < len(self.lines), f'Choice without gather: {number}'
                    number2, raw2 = self.lines[self.i]
                    self.i += 1
                    line = raw2.strip()
                    if line == '-':
                        break
                    assert line.startswith('* '), (number2, line)
                nodes.append(('choice', number, options))
            elif line.startswith('~ '):
                match = re.fullmatch(r'~ (\w+) = (.+)', line)
                assert match, (number, line)
                nodes.append(('set', number, (match[1], match[2])))
            elif line.startswith('-> '):
                nodes.append(('divert', number, line[3:]))
            else:
                assert re.match(r'[^:{}]+(?:\([^)]*\))?: .+', line), (number, line)
                nodes.append(('text', number, line))
        return nodes


state, knots, current, body = {}, {}, None, []
entry = None
for number, raw in LINES:
    line = raw.strip()
    if line.startswith('VAR '):
        match = re.fullmatch(r'VAR (\w+) = (.+)', line)
        assert match and current is None
        state[match[1]] = expression(match[2], state)
    elif line.startswith('=== '):
        if current:
            knots[current] = Parser(body).sequence()
        current, body = line.strip('= '), []
        assert current not in knots, f'Duplicate knot {current}'
    elif current:
        body.append((number, raw))
    elif line.startswith('-> '):
        entry = line[3:]
    else:
        assert not line or line.startswith('//'), (number, line)
knots[current] = Parser(body).sequence()
assert entry in knots
assert (ROOT/'Assets/Ink/chernaya-melnitsa.ink').read_text().splitlines()[-1] == 'INCLUDE s01e01.ink'

coverage, choice_groups = set(), {}


def walk(nodes, record):
    if not nodes:
        raise AssertionError('Route fell through without END')
    kind, number, value = nodes[0]
    tail = nodes[1:]
    coverage.add(number)
    if kind == 'divert':
        if value == 'END':
            return [record]
        assert value in knots and value not in record['knots'], value
        record['knots'].append(value)
        return walk(knots[value], record)
    if kind == 'set':
        key, source = value
        assert key in record['state'], key
        record['state'][key] = expression(source, record['state'])
    elif kind == 'if':
        for index, (condition, branch) in enumerate(value):
            if condition == 'else' or expression(condition, record['state']):
                record['conditions'].append((number, index))
                return walk(branch + tail, record)
    elif kind == 'choice':
        choice_groups[number] = [item[0] for item in value]
        routes = []
        for choice_id, label, branch in value:
            fork = copy.deepcopy(record)
            fork['choices'].append(choice_id)
            fork['choice_states'].append(copy.deepcopy(record['state']))
            fork['displayed'].append(label)
            routes.extend(walk(branch + tail, fork))
        return routes
    else:
        record['lines'].append(number)
        speaker, content = value.split(':', 1)
        if speaker not in ('Локация', 'Музыка', 'Звук'):
            record['displayed'].append(content)
    return walk(tail, record)


routes = walk(knots[entry], dict(state=state, knots=[entry], choices=[], choice_states=[], conditions=[], lines=[], displayed=[]))
assert len(routes) == 72, len(routes)
assert len(choice_groups) == 5 and sum(map(len, choice_groups.values())) == 12

def check_route(route):
    """Check final and intermediate continuity, not just ending reachability."""
    assert len(route['choices']) == 5
    assert len(route['choice_states']) == 5
    solo = route['choices'][1] == 'go_alone'
    released = route['choices'][2] == 'open_lada_sack'
    take = route['choices'][3] == 'take_promises'
    before_c3, before_c4, before_c5 = route['choice_states'][2:]
    assert before_c3['flour_breath'] == solo, 'C2 exposure lost before C3'
    assert before_c4['letter_read'] == released
    assert before_c4['flour_breath'] == (solo and not released), 'C3 must release Mitya coercion'
    assert before_c5['flour_breath'] == (take or (solo and not released)), 'C4 exposure mismatch'
    assert before_c5['shared_burden'] == (not take)
    assert not any(s['public_truth'] for s in route['choice_states']), 'Premature public disclosure'
    ending = route['state']['ending']
    assert route['knots'][-1] == f'ending_{ending}_scene'
    assert route['state']['public_truth'] == (ending == 'road')
    if ending == 'road':
        assert route['state']['letter_read'] and not route['state']['flour_breath']
        rested = any(line.strip().startswith('Лада осталась у реки с Настасьей.')
                     for line in route['displayed'])
        assert rested == (released and take), 'Foreign coercion needs the recovery scene'
    else:
        assert route['state']['letter_read'] == released
        assert route['state']['flour_breath'] == before_c5['flour_breath']
    return ending


counts = {}
for route in routes:
    ending = check_route(route)
    words = len(re.findall(r"[^\W\d_]+(?:[’-][^\W\d_]+)*", ' '.join(route['displayed']), re.UNICODE))
    counts.setdefault(ending, []).append(words)
assert set(counts) == {'road', 'silence', 'keeper'}
assert all(len(v) == 24 for v in counts.values())

characters = {'Лада': ('maincharacter', 'coat'), 'Яков': ('яков', 'work'),
              'Настасья': ('настасья', 'wool'), 'Савелий': ('савелий', 'raincoat'),
              'Митя': ('митя', 'jacket')}
locations, audio, variants = set(), set(), set()
for number, raw in LINES:
    line = raw.strip()
    match = re.fullmatch(r'(Локация|Музыка|Звук): (.+)', line)
    if match:
        kind, selector = match.groups()
        assert selector == unicodedata.normalize('NFC', selector).lower()
        if kind == 'Локация':
            assert (ROOT/f'Assets/Locations/{selector}.png').is_file(), (number, selector)
            locations.add(selector)
        else:
            assert sum((ROOT/f'Assets/Audio/{selector}.{ext}').is_file() for ext in ('wav','ogg','mp3')) == 1
            audio.add(selector)
    match = re.fullmatch(r'(Лада|Яков|Настасья|Савелий|Митя)(?: \((\w+)\))?: .+', line)
    if match:
        name, variant = match.groups()
        character, outfit = characters[name]
        variant = variant or 'main'
        path = f'Assets/Characters/{character}/view/whole/{outfit}/{variant}.png'
        assert (ROOT/path).is_file(), (number, path)
        variants.add(path)
assert len(locations) == 13 and len(audio) == 8
assert not (ROOT/'Assets/Characters/лада').exists()
unreached = [n for n, raw in LINES if re.match(r'\s*(?:\.\.\.|Лада|Яков|Настасья|Савелий|Митя|Локация|Музыка|Звук).*:',raw) and n not in coverage]
assert not unreached, unreached


def check_episode_covers(definition, root):
    """Check this story's plain Unity-YAML episode block, without importing Unity."""
    block = definition.split('  _episodes:\n', 1)[1].split('  _videoAliases:', 1)[0]
    entries = re.split(r'^  - _id: ', block, flags=re.MULTILINE)[1:]
    assert entries, 'Missing episodes'
    covers = {}
    for episode in entries:
        episode_id = episode.splitlines()[0].strip()
        assert episode_id not in covers, 'Duplicate episode ID'
        assigned = re.findall(r'^    _catalogCover: (.+)$', episode, re.MULTILINE)
        assert len(assigned) == 1, f'Missing/duplicate cover for {episode_id}'
        filename = assigned[0].strip()
        assert re.fullmatch(r'[A-Za-z0-9_.-]+\.png', filename) and '..' not in filename, filename
        data = (root / 'Config/EpisodeCovers' / filename).read_bytes()
        assert data[:8] == b'\x89PNG\r\n\x1a\n', filename
        assert data != (root / 'Config/cover.png').read_bytes(), 'Do not duplicate the story cover'
        covers[episode_id] = filename
    return covers


episode_covers = check_episode_covers((ROOT/'Assets/chernaya-melnitsa.asset').read_text(), ROOT)
assert episode_covers == {'s01e01': 's01e01.png'}
assert (ROOT/'Config/EpisodeCovers/s01e01.png').read_bytes() == (ROOT/'Assets/Locations/bg11-mill-undercroft.png').read_bytes()
result = {'method':'static subset interpreter; Unity/Ink compiler not run',
          'source_sha256':hashlib.sha256(TEXT.encode()).hexdigest(),
          'routes':len(routes),'choice_groups':len(choice_groups),'options':12,
          'endings':{key:{'routes':len(values),'words_min':min(values),'words_max':max(values)} for key,values in counts.items()},
          'locations':len(locations),'audio':len(audio),'exact_character_files':len(variants),
          'unreached_dialogue':unreached, 'episode_covers':episode_covers}
print(json.dumps(result, ensure_ascii=False, indent=2))
