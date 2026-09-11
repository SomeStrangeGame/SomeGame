"""Read-only audit of this story's restricted Ink subset; NOT an Ink compiler.

Enumerates source branches and checks inventory, chronology and asset addresses.
Unsupported syntax fails closed. Run with python3; only stdlib is required.
"""
import ast
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import struct
import sys

PROJECT = Path(__file__).resolve().parents[1]
REPO = PROJECT.parents[1]
SOURCE = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else PROJECT / 'Assets/Ink/s01e01.ink'
RAW = SOURCE.read_text(encoding='utf-8')
LINES = [(i, s.strip(), len(s) - len(s.lstrip()))
         for i, s in enumerate(RAW.splitlines(), 1)
         if s.strip() and not s.lstrip().startswith('//')]
LABELS = set()


def expression(text, state):
    node = ast.parse(text.replace('&&', ' and '), mode='eval').body

    def visit(n):
        if isinstance(n, ast.Constant) and type(n.value) in (int, bool):
            return n.value
        if isinstance(n, ast.Name):
            return {'true': True, 'false': False, **state}[n.id]
        if isinstance(n, ast.BinOp) and isinstance(n.op, ast.Add):
            return visit(n.left) + visit(n.right)
        if isinstance(n, ast.Compare) and len(n.ops) == 1:
            a, b = visit(n.left), visit(n.comparators[0])
            if isinstance(n.ops[0], ast.GtE):
                return a >= b
            if isinstance(n.ops[0], ast.Eq):
                return a == b
        if isinstance(n, ast.BoolOp) and isinstance(n.op, ast.And):
            return all(visit(v) for v in n.values)
        raise AssertionError('Unsupported expression: ' + text)

    return visit(node)


def parse_sequence(pos, stop):
    result = []
    while pos < len(LINES) and not stop(LINES[pos]):
        line, text, indent = LINES[pos]
        if text.startswith('{'):
            assert text.endswith(':'), (line, text)
            yes, pos = parse_sequence(pos + 1, lambda x: x[2] == indent and x[1] in ('- else:', '}'))
            no = []
            if LINES[pos][1] == '- else:':
                no, pos = parse_sequence(pos + 1, lambda x: x[2] == indent and x[1] == '}')
            assert LINES[pos][1] == '}', line
            result.append(('if', line, text[1:-1], yes, no))
            pos += 1
        elif text.startswith('* '):
            choices = []
            while pos < len(LINES) and LINES[pos][1].startswith('* ') and LINES[pos][2] == indent:
                choice_line, header, _ = LINES[pos]
                m = re.fullmatch(r'\* \((\w+)\) (?:\{(.+)\} )?\[(.+)\]', header)
                assert m, (choice_line, header)
                label, condition, label_text = m.groups()
                assert label not in LABELS, label
                LABELS.add(label)
                body, pos = parse_sequence(pos + 1, lambda x: x[2] <= indent and
                                           (x[1].startswith('* ') or x[1] == '-' or x[1].startswith('===')))
                choices.append((choice_line, label, condition or 'true', label_text, body))
            result.append(('choices', line, choices))
            if pos < len(LINES) and LINES[pos][1] == '-':
                pos += 1
        elif text.startswith('-> '):
            assert re.fullmatch(r'-> \w+', text), (line, text)
            result.append(('jump', line, text[3:]))
            pos += 1
        elif text.startswith('VAR ') or text.startswith('~ '):
            m = re.fullmatch(r'(?:VAR|~) (\w+) = (.+)', text)
            assert m, (line, text)
            result.append(('set', line, *m.groups()))
            pos += 1
        else:
            assert ':' in text and text[0] not in '{}*-', (line, text)
            result.append(('text', line, text))
            pos += 1
    return result, pos


KNOTS = {}
entry, cursor = parse_sequence(0, lambda x: x[1].startswith('==='))
while cursor < len(LINES):
    line, header, _ = LINES[cursor]
    m = re.fullmatch(r'=== (\w+) ===', header)
    assert m and m[1] not in KNOTS, (line, header)
    body, cursor = parse_sequence(cursor + 1, lambda x: x[1].startswith('==='))
    KNOTS[m[1]] = body

ENDINGS = {'keeper_ending', 'iron_ending', 'nameless_ending'}
COMMANDS = {'Название', 'Серия', 'Жанры', 'Аннотация', 'Статы', 'Локация', 'Музыка', 'Звук', 'Уведомление'}
CAST = {'Яр': 'maincharacter', 'Лада': 'лада', 'Весна': 'весна', 'Тихон': 'тихон', 'Мирон': 'мирон'}
SEEN_LINES, SEEN_CHOICES = set(), set()
RESOURCES = {'Locations': set(), 'Audio': set(), 'Choices': set()}
ROUTES = []


def resource(kind, name, extension):
    assert (PROJECT / 'Assets' / kind / (name + extension)).is_file(), (kind, name)
    RESOURCES[kind].add(name)


def check_text(line, text, state, decisions, knots):
    speaker, body = text.split(':', 1)
    if speaker == 'Локация':
        resource('Locations', body.strip(), '.png')
    elif speaker in ('Музыка', 'Звук'):
        resource('Audio', body.strip(), '.wav')
    elif speaker != '...' and speaker not in COMMANDS:
        m = re.fullmatch(r'(\w+) \((\w+), (\w+)\)', speaker)
        assert m and m[1] in CAST, (line, 'unknown speaker', speaker)
        character, outfit, variant = m.groups()
        root = PROJECT / 'Assets/Characters' / CAST[character] / 'view/whole' / outfit
        assert (root / (variant + '.png')).is_file() and (root / 'main.png').is_file(), (line, root, variant)
        current = knots[-1]
        if character == 'Лада':
            if current in ('village_edge', 'council', 'charcoal_boundary', 'bell_tower', 'drowned_shrine'):
                assert variant != 'wounded', (line, 'injury before attack')
            if current in ('root_archive', 'bird_glade', 'heart_oak') or current in ENDINGS:
                assert variant == 'wounded', (line, 'injury disappeared')
        if character == 'Тихон':
            assert current in ('bell_tower', 'drowned_shrine'), (line, 'Tikhon teleported')
        if character == 'Весна':
            assert current not in ENDINGS | {'bird_glade', 'heart_oak'}, (line, 'Vesna teleported')
        if character == 'Мирон' and current == 'heart_oak':
            assert state['spoke_miron'], (line, 'silent Miron speaks')
    if 'Железный гвоздь в руке' in body or 'вбил им межевой гвоздь' in body:
        assert state['has_iron'], (line, 'nail not acquired', decisions)
    if 'Бронзовый язычок тихо дрогнул' in body or 'Яр коснулся язычка' in body:
        assert state['has_bronze'], (line, 'clapper not acquired', decisions)
    if 'приложил спасённую карту' in body or 'развернул спасённую трубку' in body:
        assert 'save_map' in decisions, (line, 'map not acquired', decisions)
    if 'Табличка за пазухой потеплела' in body:
        assert state['kept_path_token'], (line, 'tablet not acquired', decisions)
    if speaker == 'Уведомление':
        if 'Карта унесена водой' in body:
            assert state['saved_lada']
        if 'Карта сохранена' in body:
            assert not state['saved_lada'] and 'save_map' in decisions
        if 'Мирон освобождён' in body:
            assert state['ending_code'] == 3 and state['miron_location'] == 3 and not state['yar_name_intact']
        if 'Мирон жив на севере' in body:
            assert state['ending_code'] == 1 and state['miron_location'] == 1
        if 'Мирон не освобождён' in body:
            assert state['ending_code'] == 2 and state['miron_location'] == 2
        return len(body.split())
    return 0 if speaker in COMMANDS else len(body.split())


def walk(nodes, state, decisions, knots, words=0):
    assert len(knots) < 20, 'Unexpected cycle'
    if not nodes:
        raise AssertionError(('Fell through without END', knots, decisions))
    kind, line, *args = nodes[0]
    tail = nodes[1:]
    SEEN_LINES.add(line)
    if kind == 'text':
        walk(tail, state, decisions, knots, words + check_text(line, args[0], state, decisions, knots))
    elif kind == 'set':
        key, value = args
        walk(tail, {**state, key: expression(value, state)}, decisions, knots, words)
    elif kind == 'if':
        condition, yes, no = args
        walk((yes if expression(condition, state) else no) + tail, state, decisions, knots, words)
    elif kind == 'choices':
        available = [c for c in args[0] if expression(c[2], state)]
        assert available, (line, 'No choices', decisions)
        for choice_line, label, _, label_text, body in available:
            SEEN_CHOICES.add(label)
            SEEN_LINES.add(choice_line)
            if '# choice_icon:' in label_text:
                label_text, icon = label_text.split('# choice_icon:')
                resource('Choices', icon.strip(), '.png')
            walk(body + tail, state.copy(), decisions + [label], knots, words + len(label_text.split()))
    elif kind == 'jump':
        target = args[0]
        if target == 'END':
            ending = [k for k in knots if k in ENDINGS]
            assert len(ending) == 1 and knots[-1] == 'cliffhanger' and len(decisions) == 8
            assert state['has_iron'] != state['has_bronze']
            assert state['has_iron'] == ('take_iron' in decisions)
            assert state['kept_path_token'] == ('keep_path_token' in decisions)
            assert state['saved_lada'] == ('save_lada' in decisions)
            if ending[0] == 'iron_ending':
                assert state['has_iron'] and not state['has_bronze'] and state['iron'] >= 2
            if ending[0] == 'keeper_ending':
                assert state['bond'] >= 2
            expected_code = {'keeper_ending': 1, 'iron_ending': 2, 'nameless_ending': 3}[ending[0]]
            assert state['ending_code'] == state['miron_location'] == expected_code
            assert state['yar_name_intact'] == (expected_code != 3)
            ROUTES.append((ending[0], words, decisions))
        else:
            assert target in KNOTS, (line, 'Unknown divert', target)
            walk(KNOTS[target], state, decisions, knots + [target], words)


# Cross-scene authored contract: the shrine's eighth name cannot also be one
# of the seven restored names. This guards the concrete numbering regression;
# narrative chronology and the meaning of magical rules still need human review.
eighth_match = re.search(r'На восьмой Яр прочёл вырезанное имя: «([^»]+)»', RAW)
assert eighth_match, 'Shrine eighth-name contract missing'
EIGHTH_NAME = eighth_match[1]
archive_choices = next(n[2] for n in KNOTS['root_archive'] if n[0] == 'choices')
confession = next(c[4] for c in archive_choices if c[1] == 'make_vesna_confess')
confession_speech = [n[2].split(':', 1)[1].strip() for n in confession
                     if n[0] == 'text' and n[2].startswith('Весна (')]
RESTORED_NAMES = [name.strip() for name in confession_speech[0].split('.') if name.strip()]
assert len(RESTORED_NAMES) == len(set(RESTORED_NAMES)) == 7, 'Expected seven unique restored names'
assert EIGHTH_NAME not in RESTORED_NAMES, 'Eighth name counted among the seven'
assert confession_speech[1].startswith('Восьмое — ' + EIGHTH_NAME + '.'), 'Known eighth name forgotten'

walk(entry, {}, [], [])


def executable_lines(nodes):
    result = set()
    for kind, line, *args in nodes:
        result.add(line)
        if kind == 'if':
            result |= executable_lines(args[1]) | executable_lines(args[2])
        elif kind == 'choices':
            for choice_line, _, _, _, body in args[0]:
                result.add(choice_line)
                result |= executable_lines(body)
    return result


expected_lines = executable_lines(entry)
for nodes in KNOTS.values():
    expected_lines |= executable_lines(nodes)
assert expected_lines == SEEN_LINES, ('Unreachable executable lines', sorted(expected_lines - SEEN_LINES))
assert len(KNOTS) == 13 and len(LABELS) == 18 and SEEN_CHOICES == LABELS
assert set(Counter(r[0] for r in ROUTES)) == ENDINGS
assert len({tuple(r[2][:-1]) for r in ROUTES}) == 192
assert len(ROUTES) == 372
for kind, ext in [('Locations', '.png'), ('Audio', '.wav'), ('Choices', '.png')]:
    files = {p.stem for p in (PROJECT / 'Assets' / kind).glob('*' + ext)}
    assert RESOURCES[kind] == files, (kind, 'unreferenced assets', files - RESOURCES[kind])

# Story-local episode-cover contract, checked without Unity or generated preview.
definition = (PROJECT / 'Assets/znak-na-dube.asset').read_text(encoding='utf-8')
episodes = definition.split('  _episodes:\n', 1)[1].split('  _videoAliases:', 1)[0]
cover_entries = re.findall(r'  - _id: (\w+)\n(.*?)(?=  - _id:|\Z)', episodes, re.S)
assert len(cover_entries) == 1 and cover_entries[0][0] == 's01e01'
episode_covers = {}
for episode_id, fields in cover_entries:
    match = re.search(r'^    _catalogCover: ([A-Za-z0-9_.-]+)$', fields, re.M)
    assert match and '..' not in match[1], (episode_id, 'invalid or missing episode cover')
    filename = match[1]
    assert filename == episode_id + '.png', (episode_id, 'unexpected cover mapping')
    cover = PROJECT / 'Config/EpisodeCovers' / filename
    data = cover.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n' and data[12:16] == b'IHDR', (cover, 'not PNG')
    width, height, depth, color_type = struct.unpack('>IIBB', data[16:26])
    assert (width, height, depth, color_type) == (1024, 1536, 8, 2), (cover, 'unexpected cover format')
    episode_covers[episode_id] = filename
assert {p.name for p in (PROJECT / 'Config/EpisodeCovers').iterdir()} == set(episode_covers.values())

# Preview must reproduce the complete linear opening, with exact attribution.
preview_root = PROJECT / 'Config/Preview'
preview = json.loads((preview_root / 'preview.json').read_text())
assert preview['schemaVersion'] == 1
card = json.loads((PROJECT / 'Config/card.json').read_text())
assert preview['storyId'] == card['storyId'] == 'znak-na-dube'
assert preview['episodeId'] == 's01e01' and preview['source'] == 'Assets/Ink/s01e01.ink'
assert preview['title'] == 'Ночелесье: Знак на дубе'
assert type(preview['estimatedReadingMinutes']) is int and 1 <= preview['estimatedReadingMinutes'] <= 5
opening = RAW.split('=== village_edge ===', 1)[1].split('* (', 1)[0]
expected_blocks, expected_characters = [], {}
for raw in opening.splitlines():
    text = raw.strip()
    if text.startswith('...:'):
        expected_blocks.append({'type': 'narration', 'text': text.split(':', 1)[1].strip()})
    match = re.fullmatch(r'(Яр|Лада) \((\w+), (\w+)\): (.+)', text)
    if match:
        speaker, outfit, variant, body = match.groups()
        character = CAST[speaker]
        if character not in expected_characters:
            filename = {'Яр': 'yar', 'Лада': 'lada'}[speaker] + '.png'
            expected_characters[character] = {'name': speaker, 'image': 'characters/' + filename}
            master = PROJECT / 'Assets/Characters' / character / 'view/whole' / outfit / (variant + '.png')
            assert (preview_root / 'characters' / filename).read_bytes() == master.read_bytes()
            expected_blocks.append({'type': 'character', 'character': character})
        expected_blocks.append({'type': 'dialogue', 'speaker': speaker, 'text': body})
assert preview['characters'] == expected_characters, 'Preview character mapping differs from canonical opening'
assert preview['blocks'] == expected_blocks, 'Preview prose/order/speaker differs from canonical opening'
assert {p.name for p in (preview_root / 'characters').iterdir()} == {'yar.png', 'lada.png'}

# Reader copy includes every authored prose/dialogue alternative in source order.
reader = (PROJECT / 'Art/SCENARIO.md').read_text()
assert hashlib.sha256(RAW.encode()).hexdigest() in reader
reader_pos = 0
for _, text, _ in LINES:
    if text.startswith('...:') or re.match(r'(Яр|Лада|Весна|Тихон|Мирон) \(', text) or text.startswith('Уведомление:'):
        prose = text.split(':', 1)[1].strip()
        found = reader.find(prose, reader_pos)
        assert found >= reader_pos, 'Readable scenario lost or reordered source prose: ' + prose
        reader_pos = found + len(prose)

# All authored GUIDs must be valid, unique within this Unity project and resolve.
guid_paths = {}
for root in [PROJECT / 'Assets', REPO / 'Packages']:
    for meta in root.rglob('*.meta'):
        match = re.search(r'^guid: (\S+)', meta.read_text(), re.M)
        if match:
            guid = match[1]
            assert re.fullmatch('[0-9a-f]{32}', guid), (meta, 'invalid GUID', guid)
            assert guid not in guid_paths, (meta, 'duplicate GUID', guid_paths.get(guid))
            guid_paths[guid] = str(meta)
for folder, pattern in [(PROJECT / 'Assets', '*.prefab'), (PROJECT / 'Assets', '*.asset')]:
    for path in folder.rglob(pattern):
        for guid in re.findall(r'(?:guid: |RootInkGuid: )([0-9a-f]+)', path.read_text()):
            assert guid in guid_paths, (path, 'unresolved GUID', guid)

print(json.dumps({
    'status': 'passed', 'scope': 'static restricted-source model, not compiled Ink',
    'source_sha256': hashlib.sha256(RAW.encode()).hexdigest(),
    'knots': len(KNOTS), 'choices': len(LABELS), 'complete_routes': len(ROUTES),
    'pre_final_combinations': len({tuple(r[2][:-1]) for r in ROUTES}),
    'unreachable_executable_lines': len(expected_lines - SEEN_LINES),
    'endings': dict(Counter(r[0] for r in ROUTES)),
    'displayed_words_including_selected_labels': [min(r[1] for r in ROUTES), max(r[1] for r in ROUTES)],
    'resources': {k: len(v) for k, v in RESOURCES.items()},
    'restored_place_names': len(RESTORED_NAMES), 'eighth_place_name': EIGHTH_NAME,
    'episode_covers': episode_covers,
    'website_preview': {'blocks': len(expected_blocks), 'characters': len(expected_characters), 'exact_opening_match': True},
    'notifications_authored': sum(1 for _, text, _ in LINES if text.startswith('Уведомление:')),
    'ending_state_contract': {'1': 'keeper / north / name retained', '2': 'iron / beyond sealed path / name retained', '3': 'exchange / present / name lost'},
    'reading_minutes_at_110_to_140_effective_wpm': [round(min(r[1] for r in ROUTES) / 140, 1), round(max(r[1] for r in ROUTES) / 110, 1)],
    'shortest_route': min(ROUTES, key=lambda r: r[1])[2],
}, ensure_ascii=False, indent=2))
