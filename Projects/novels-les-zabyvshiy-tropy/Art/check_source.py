#!/usr/bin/env python3
"""Read-only audit of this story's bounded Ink subset; NOT an Ink compiler.

Understands includes, VAR, assignments, named knots/choices, gathers, multiline
conditionals and diverts. Unknown syntax fails closed. Uses no Unity or packages.
"""
import ast
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import struct

ROOT = Path(__file__).resolve().parents[1]
INK = ROOT / 'Assets/Ink'


def require(ok, message):
    if not ok:
        raise ValueError(message)


def value(source, state):
    node = ast.parse(source.replace('&&', ' and ').replace('||', ' or '), mode='eval').body

    def visit(n):
        if isinstance(n, ast.Constant) and type(n.value) in (str, int, bool):
            return n.value
        if isinstance(n, ast.Name):
            return {'true': True, 'false': False, **state}[n.id]
        if isinstance(n, ast.BinOp) and isinstance(n.op, ast.Add):
            return visit(n.left) + visit(n.right)
        if isinstance(n, ast.BoolOp):
            return (all if isinstance(n.op, ast.And) else any)(visit(v) for v in n.values)
        if isinstance(n, ast.Compare) and len(n.ops) == 1:
            left, right = visit(n.left), visit(n.comparators[0])
            if isinstance(n.ops[0], ast.Eq):
                return left == right
            if isinstance(n.ops[0], ast.Gt):
                return left > right
            if isinstance(n.ops[0], ast.GtE):
                return left >= right
        raise ValueError(f'Unsupported expression: {source}')

    return visit(node)


def parse(lines):
    pos = 0

    def block(stops=()):
        nonlocal pos
        nodes = []
        while pos < len(lines):
            source, line = lines[pos]
            if any(line.startswith(s) for s in stops):
                break
            pos += 1
            if line.startswith('{') and line.endswith(':'):
                branches = []
                condition = line[1:-1]
                while True:
                    branches.append((condition, block(('|', '}'))))
                    require(pos < len(lines), f'{source}: unclosed conditional')
                    _, boundary = lines[pos]
                    pos += 1
                    if boundary == '}':
                        break
                    require(boundary.startswith('|') and boundary.endswith(':'), source)
                    condition = boundary[1:-1]
                nodes.append(('if', branches))
            elif line.startswith('*'):
                options = []
                while True:
                    match = re.fullmatch(r'\* \((\w+)\) \[(.+) # choice_icon:([\w-]+)\]', line)
                    require(match, f'{source}: unsupported choice {line}')
                    label, title, icon = match.groups()
                    options.append((label, title, icon, block(('*', '-'))))
                    require(pos < len(lines), f'{source}: choice without gather')
                    source, line = lines[pos]
                    pos += 1
                    if line == '-':
                        break
                    require(line.startswith('*'), f'{source}: unsupported gather')
                nodes.append(('choice', options))
            elif line.startswith('~ '):
                name, expression = line[2:].split('=', 1)
                nodes.append(('set', name.strip(), expression.strip()))
            elif line.startswith('-> '):
                nodes.append(('goto', line[3:]))
            else:
                require(re.match(r'^(?:\.\.\.|[А-ЯЁ][А-Яа-яЁё ]*)(?: \([^)]*\))?: .+', line),
                        f'{source}: unsupported line {line}')
                nodes.append(('text', source, line))
        return nodes

    result = block()
    require(pos == len(lines), 'Unparsed source')
    return result


def audit():
    card = json.loads((ROOT / 'Config/card.json').read_text())
    require(card['storyId'] == 'les-zabyvshiy-tropy', 'Story ID mismatch')
    require((ROOT / 'Config' / card['cover']).is_file(), 'Missing cover')
    definition = (ROOT / 'Assets/les-zabyvshiy-tropy.asset').read_text()
    main = re.search(r'_mainCharacter: "(.+)"', definition).group(1)
    defaults = dict(re.findall(r'_character: "(.+)"\n\s+_clothes: (\S+)', definition))
    includes = re.findall(r'^INCLUDE (\S+)$', (INK / (card['storyId'] + '.ink')).read_text(), re.M)
    require(includes == [f's01e{i:02}.ink' for i in range(1, 7)], 'Include order')
    require(re.findall(r'- _id: (s\d+e\d+)', definition) == [Path(p).stem for p in includes],
            'Definition episode order')
    # Story-local requirement, stricter than the SDK's optional episode cover.
    episode_section = definition.split('  _episodes:\n', 1)[1].split('  _videoAliases:', 1)[0]
    episode_covers = {}
    for episode in re.split(r'(?=^  - _id: )', episode_section, flags=re.M):
        if not episode.strip():
            continue
        episode_id = re.search(r'^  - _id: (\S+)$', episode, re.M).group(1)
        bindings = re.findall(r'^    _catalogCover: (\S+)$', episode, re.M)
        require(bindings == [episode_id + '.png'], f'{episode_id}: missing/incorrect cover binding')
        path = ROOT / 'Config/EpisodeCovers' / bindings[0]
        require(path.is_file(), f'{episode_id}: missing episode cover')
        data = path.read_bytes()
        require(data[:8] == b'\x89PNG\r\n\x1a\n' and data[12:16] == b'IHDR',
                f'{episode_id}: invalid PNG header')
        require(len(data) >= 33 and struct.unpack('>II', data[16:24]) == (1024, 1536),
                f'{episode_id}: expected portrait 1024x1536')
        episode_covers[episode_id] = hashlib.sha256(data).hexdigest()
    require(len(episode_covers) == 6 and len(set(episode_covers.values())) == 6,
            'Expected six distinct episode covers')
    require(hashlib.sha256((ROOT / 'Config' / card['cover']).read_bytes()).hexdigest()
            not in episode_covers.values(), 'Episode cover duplicates story cover')
    initial, raw, current = {}, {'entry': []}, 'entry'
    locations, audio, icons, character_files, labels = set(), set(), set(), set(), set()
    source_hash = hashlib.sha256()
    for name in [card['storyId'] + '.ink', *includes]:
        source_hash.update(name.encode())
        source_hash.update((INK / name).read_bytes())
    for name in includes:
        for number, original in enumerate((INK / name).read_text().splitlines(), 1):
            line = original.strip()
            source = f'{name}:{number}'
            if not line or line.startswith('//'):
                continue
            if line.startswith('VAR '):
                var, expression = line[4:].split('=', 1)
                var = var.strip()
                require(var not in initial, f'Duplicate variable {var}')
                initial[var] = value(expression.strip(), initial)
                continue
            if line.startswith('=== '):
                current = line[4:-4]
                require(current not in raw, f'Duplicate knot {current}')
                raw[current] = []
                continue
            raw[current].append((source, line))
            if line.startswith('*'):
                label = re.search(r'\((\w+)\)', line).group(1)
                require(label not in labels, f'Duplicate choice label {label}')
                labels.add(label)
                icon = re.search(r'choice_icon:([\w-]+)', line).group(1)
                require((ROOT / 'Assets/Choices' / (icon + '.png')).is_file(), f'Missing {icon}')
                icons.add(icon)
            if ':' not in line or line[0] in '{|*':
                continue
            prefix, payload = line.split(':', 1)
            payload = payload.strip()
            if prefix == 'Локация':
                require((ROOT / 'Assets/Locations' / (payload + '.png')).is_file(), source)
                locations.add(payload)
            elif prefix in ('Музыка', 'Звук', 'Звуки окружения'):
                matches = [ROOT / 'Assets/Audio' / (payload + ext) for ext in ('.wav', '.mp3', '.ogg')]
                require(sum(p.is_file() for p in matches) == 1, f'{source}: missing/ambiguous audio')
                audio.add(payload)
            elif prefix != '...':
                speaker = re.fullmatch(r'([^()]+?)(?: \((.+)\))?', prefix)
                require(speaker, f'{source}: invalid speaker')
                character, arguments = speaker.groups()
                require(character in defaults, f'{source}: unknown speaker {character}')
                asset_id = 'maincharacter' if character == main else character.lower()
                outfit = defaults[character]
                candidates = arguments.split(', ') if arguments else []
                if candidates and candidates[0] == outfit:
                    candidates.pop(0)
                require(len(candidates) <= 1, f'{source}: unsupported presentation')
                variant = candidates[0] if candidates else 'main'
                for state in {'main', variant}:
                    path = ROOT / 'Assets/Characters' / asset_id / 'view/whole' / outfit / (state + '.png')
                    require(path.is_file(), f'{source}: missing runtime character {path}')
                    character_files.add(path.relative_to(ROOT).as_posix())
    require(not (labels & initial.keys()), 'Choice/global variable name collision')
    require(not (raw.keys() & initial.keys()), 'Knot/global variable name collision')
    knots = {name: parse(lines) for name, lines in raw.items()}
    routes = []

    def walk(pending, state, choices, spoken, visited, visible_choices):
        while pending:
            node, *pending = pending
            kind = node[0]
            if kind == 'text':
                spoken = spoken + [node[2]]
            elif kind == 'set':
                require(node[1] in state, f'Undeclared {node[1]}')
                result = value(node[2], state)
                require(type(result) is type(state[node[1]]), f'Type mismatch {node[1]}')
                state = {**state, node[1]: result}
            elif kind == 'if':
                for condition, body in node[1]:
                    if condition == 'else' or value(condition, state):
                        pending = body + pending
                        break
            elif kind == 'choice':
                shown = sum(len(title.split()) for _, title, _, _ in node[1])
                for label, _, _, body in node[1]:
                    walk(body + pending, state.copy(), choices + [label], spoken, visited, visible_choices + shown)
                return
            elif kind == 'goto':
                target = node[1]
                if target == 'END':
                    routes.append((state, choices, spoken, visited, visible_choices))
                    return
                require(target in knots and target not in visited, f'Missing/cyclic divert {target}')
                visited = visited + [target]
                pending = knots[target]
        raise ValueError('Route falls off without END')

    walk(knots['entry'], initial, [], [], [], 0)
    endings, lengths = Counter(), []
    for state, choices, spoken, visited, option_words in routes:
        require(len(choices) == 5, 'Route must contain five decisions')
        require(visited[:6] == [f'LZT_s01e{i:02}' for i in range(1, 7)], 'Episode continuity')
        ending = visited[-1]
        expected = ('ending_shared' if state['final_choice'] == 'share' else
                    'ending_white_map' if state['final_choice'] == 'bind' and state['keep_map'] and state['map_debt'] >= 2 else
                    'ending_nameless')
        require(ending == expected, 'Ending predicate mismatch')
        endings[ending] += 1
        require(spoken.count('...: КОНЕЦ СЕРИИ') == 5, 'Episode boundary marker count')
        require(spoken.count('...: КОНЕЦ ИСТОРИИ') == 1, 'Story completion marker count')
        route_text = '\n'.join(spoken)
        melody = 'Филя тихо насвистел четыре ноты'
        repeat = 'Филя повторил ноты'
        require(melody in route_text and repeat in route_text and
                route_text.index(melody) < route_text.index(repeat), 'Melody repeated before introduction')
        require('Меня в чертеже тоже нет' in route_text and
                route_text.index('Меня в чертеже тоже нет') < route_text.index('Лес выдохнул'),
                'Missing warning about Lada before final resolution')
        if state['honesty'] < 2:
            require('убрала в глубокий карман дождевика' in route_text,
                    'Closed jar has no carrying arrangement at crossing')
        if ending == 'ending_shared':
            promises = route_text.split('Каждый отдаст не имя', 1)[-1].split('Лес выдохнул', 1)[0]
            for memory in ('как Яр резал яблоко', 'как Лада', 'как Филя остался',
                           'Ника всегда возвращается', 'как Ася назвала'):
                require(memory in promises, f'Incomplete shared-memory circle: {memory}')
        if state['trust_lada'] > 0:
            require('поймала тебя на мосту' not in route_text and 'вытащила меня с моста' not in route_text,
                    'Impossible bridge rescue callback')
        require('В стене открылась дверь' in route_text, 'Missing reconverged exit')
        require(('последний штрих рассыпался' in route_text) ==
                (state['final_choice'] == 'bind' and ending == 'ending_nameless'), 'Bind failure explanation')
        displayed = [s.split(':', 1)[1] for s in spoken if s.split(':', 1)[0] not in
                     ('Локация', 'Музыка', 'Звук', 'Звуки окружения') and s not in
                     ('...: КОНЕЦ СЕРИИ', '...: КОНЕЦ ИСТОРИИ')]
        lengths.append(sum(len(s.split()) for s in displayed) + option_words)
    require(len(routes) == 72, 'Expected all 3×2×2×2×3 choice combinations')
    return {'source_sha256': source_hash.hexdigest(), 'routes': len(routes), 'endings': dict(endings),
            'episode_cover_sha256': episode_covers,
            'decisions_per_route': 5, 'options_total': len(labels), 'locations': len(locations),
            'choice_icons': len(icons), 'audio_ids': len(audio), 'character_files_resolved': len(character_files),
            'source_word_units': sum(len((INK / p).read_text().split()) for p in includes),
            'displayed_word_units_including_offered_choices': [min(lengths), max(lengths)],
            'limitations': 'Static subset interpreter, not Ink compilation or Unity/runtime/save/visual proof.'}


if __name__ == '__main__':
    print(json.dumps(audit(), ensure_ascii=False, indent=2))
