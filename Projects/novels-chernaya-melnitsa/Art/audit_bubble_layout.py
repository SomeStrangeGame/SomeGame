"""Static font/geometry preflight, NOT a Unity or visual acceptance test.

Requires Pillow (available in the Codex bundled Python runtime). Measures the
actual shipped font and all source dialogue/choice strings. Runtime fitting,
characters, safe areas and pressed states still require a fresh APK replay.
"""
import contextlib
import io
import json
import math
import re
import runpy
from pathlib import Path

from PIL import Image, ImageFont

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT.parents[1] / 'Packages/NovelsContentSdk/BaseUI/Base/bubble/screen.prefab'
PREFAB = ROOT / 'Assets/Presentation/bubble/screen-variant.prefab'
FONT = ROOT / 'Assets/Presentation/Fonts/liberationsans-regular.ttf'


def properties(text, expected_guid='513fc0c5b09af4d9f8fc3bf5ce420c8e'):
    pairs = re.findall(
        r'- target: \{fileID: (\d+), guid: ([a-f0-9]+), type: 3\}\n'
        r'      propertyPath: ([^\n]+)\n      value:([^\n]*)', text)
    result = {}
    for file_id, guid, key, value in pairs:
        identity = (file_id, key)
        assert identity not in result, f'Duplicate override: {identity}'
        assert guid == expected_guid
        result[identity] = value.strip()
    return result


def wrapped_height(text, width, size):
    font = ImageFont.truetype(str(FONT), size)
    lines = ['']
    for word in text.split():
        assert font.getlength(word) <= width, f'Unbreakable word: {word}'
        candidate = (lines[-1] + ' ' + word).strip()
        if lines[-1] and font.getlength(candidate) > width:
            lines.append(word)
        else:
            lines[-1] = candidate
    # Deliberately allow 20% beyond FreeType ascent/descent for Unity's layout.
    return math.ceil(len(lines) * sum(font.getmetrics()) * 1.2)


def main():
    base = BASE.read_text()
    props = properties(PREFAB.read_text())
    ids = set(re.findall(r'^--- !u!\d+ &(\d+)$', base, re.M))
    assert all(file_id in ids for file_id, _ in props), 'Missing inherited target'
    body_ids = ['4144768857679889277', '7503267815130482102',
                '2080254210932930884', '582945343224599575', '4739581548258826096']
    panel_ids = ['144265933838496173', '235914278084610185',
                 '526757144306610934', '1573244290483288248', '2657329079934558515']
    for file_id in body_ids:
        assert props[file_id, 'm_FontData.m_FontSize'] == '22'
    for file_id in panel_ids:
        assert props[file_id, 'm_Type'] == '1'
        assert props[file_id, 'm_PixelsPerUnitMultiplier'] == '8'
    assert props['4967375187369194816', 'm_AnchoredPosition.y'] == '210'
    # User requested raised/enlarged characters and lower text after Ink:48
    # exposed face occlusion. Keep the coordinated story-local layout explicit.
    named_roots = ['5261210171850832744', '4176557829553505523',
                   '4291775287375605381']
    for file_id in named_roots:
        assert props[file_id, 'm_AnchoredPosition.y'] == '-180'
    assert props['9022507678251648235', 'm_AnchoredPosition.y'] == '-55'
    assert props['484171828424592385', 'm_SizeDelta.y'] == '96'
    assert props['4810358377489935136', 'm_FontData.m_FontSize'] == '20'
    assert props['4810358377489935136', 'm_FontData.m_Alignment'] == '4'
    assert props['2090547154547940056', 'm_PixelsPerUnitMultiplier'] == '8'
    assert 'm_ReferenceResolution: {x: 465, y: 1024}' in base
    assert 'm_ScreenMatchMode: 1' in base  # Expand: logical height >= 1024.
    for kind in ['character', 'nocharacter']:
        meta = (ROOT / f'Assets/Presentation/bubble/sprites/{kind}/bubble.png.meta').read_text()
        assert 'spriteBorder: {x: 240, y: 120, z: 240, w: 120}' in meta
    ink = (ROOT / 'Assets/Ink/s01e01.ink').read_text().splitlines()
    dialogue, named, labels = [], [], []
    speakers = {}
    for number, line in enumerate(ink, 1):
        match = re.fullmatch(r'\s*(?:\.\.\.|Лада|Яков|Настасья|Савелий|Митя)(?: \(\w+\))?: (.+)', line)
        if match:
            dialogue.append((wrapped_height(match[1], 380, 22), number))
            speakers.setdefault(match[1], set()).add(not line.strip().startswith('...:'))
            if not line.strip().startswith('...:'):
                named.append((wrapped_height(match[1], 380, 22), number))
        match = re.fullmatch(r'\s*\*.*\[(.*)\]', line)
        if match:
            labels.append((wrapped_height(match[1], 340, 20), number))
    assert len(labels) == 12
    body_height, body_line = max(dialogue)
    label_height, label_line = max(labels)
    narrator_top = 1024 / 2 - 210 + 55 + 32
    named_top = 1024 / 2 - 210 + 180 + 32
    # Measure actual pre-choice text on all 72 routes: C5 can follow Lada or
    # Nastasiya, not just the narrator. Never assume all choices use one root.
    with contextlib.redirect_stdout(io.StringIO()):
        story = runpy.run_path(str(ROOT / 'Art/audit_story.py'))
    label_options = dict(re.findall(r'^\s*\* \((\w+)\) \[(.+)\]$', '\n'.join(ink), re.M))
    choice_counts = {choice: len(group) for group in story['choice_groups'].values()
                     for choice in group}
    choice_bottoms = []
    for route in story['routes']:
        for choice in route['choices']:
            index = route['displayed'].index(label_options[choice])
            previous = route['displayed'][index - 1].strip()
            assert len(speakers[previous]) == 1, 'Ambiguous pre-choice speaker'
            is_named = next(iter(speakers[previous]))
            top = named_top if is_named else narrator_top
            height = wrapped_height(previous, 380, 22)
            choice_bottoms.append(top + height + choice_counts[choice] * (96 + 12) + 5)
    assert len(choice_bottoms) == 72 * 5
    bottom = max(choice_bottoms)
    assert bottom <= 1024 - 48, (bottom, 'insufficient bottom clearance')
    assert narrator_top + body_height + 40 <= 1024 - 48
    assert label_height <= 96 + 10 - 2 * 139 / 8, 'label intersects button border'
    assert 240 / 8 < 80 / 2 and 120 / 8 < 80 / 2, 'panel border intersects text'
    named_height, named_line = max(named)
    named_bottom = named_top + named_height + 40
    assert named_bottom <= 1024 - 48
    character_base = (ROOT.parents[1] / 'Packages/NovelsContentSdk/BaseUI/Base/character/screen.prefab').read_text()
    character_props = properties(
        (ROOT / 'Assets/Presentation/character/screen-variant.prefab').read_text(),
        'f5229a1934c0e49869f12b3adf69a449')
    character_ids = set(re.findall(r'^--- !u!\d+ &(-?\d+)$', character_base, re.M))
    assert all(file_id in character_ids for file_id, _ in character_props)
    viewport = '4967375187369194816'
    assert character_props[viewport, 'm_AnchoredPosition.y'] == '150'
    assert character_props[viewport, 'm_LocalPosition.y'] == '150'
    for axis in ('x', 'y'):
        assert character_props[viewport, f'm_LocalScale.{axis}'] == '1.2'
    assert 'm_ReferenceResolution: {x: 465, y: 1024}' in character_base
    # Whole sprites retain their original transparent registration. Use alpha
    # bounds for a rough top-of-head reserve, not as a face/cropping visual pass.
    character_tops = []
    for path in (ROOT / 'Assets/Characters').rglob('*.png'):
        with Image.open(path) as sprite:
            assert sprite.size == (768, 1280)
            alpha_top = sprite.getchannel('A').getbbox()[1]
        for height in (1024, 1034, 1200):
            top = -150 + alpha_top * height / 1280 * 1.2
            header_top = height / 2 - 210 + 180 - 25
            assert top >= 80, 'raised character enters top safe margin'
            assert top + 140 <= header_top, 'estimated head reserve intersects named header'
            character_tops.append(top)
    assert len(character_tops) == 11 * 3
    # Original settings exceed this conservative viewport budget and give the
    # labels too little dark interior. Keep the reproduction guard explicit.
    old_heights = [wrapped_height(re.fullmatch(r'\s*\*.*\[(.*)\]', line)[1], 340, 25)
                   for line in ink if re.fullmatch(r'\s*\*.*\[(.*)\]', line)]
    assert max(old_heights) > 65 + 10 - 2 * 139 / 5
    print(json.dumps({'method': 'static conservative font/geometry estimate; runtime pending',
                      'dialogue_strings': len(dialogue), 'choice_labels': len(labels),
                      'max_body_height': body_height, 'max_body_line': body_line,
                      'max_label_height': label_height, 'max_label_line': label_line,
                      'named_strings': len(named), 'max_named_height': named_height,
                      'max_named_line': named_line, 'named_panel_bottom': named_bottom,
                      'choice_route_contexts': len(choice_bottoms),
                      'character_scale': 1.2, 'character_viewport_y': 150,
                      'estimated_character_top_range': [min(character_tops), max(character_tops)],
                      'worst_case_bottom': bottom, 'logical_height': 1024,
                      'bottom_clearance': 1024 - bottom}, indent=2))


if __name__ == '__main__':
    main()
