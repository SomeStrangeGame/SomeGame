"""Read-only source/font/alpha preflight; NOT Unity layout or Player evidence."""
import contextlib
import hashlib
import importlib.util
import io
import json
import math
import re
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageFont

ROOT = Path(__file__).resolve().parents[2]
PREFAB = ROOT / 'Assets/Presentation/bubble/screen-variant.prefab'
FONT = ROOT / 'Assets/Presentation/bubble/liberationsans-regular.ttf'
HASH = 'fc2e8e17bf0226d9b6fffc8c7f2209e80c0fa3c22638da4e6bc95d1c751364cc'


def blocks(text):
    pairs = re.findall(r'^--- !u!\d+ &(\d+)\n(.*?)(?=^--- !u!|\Z)', text, re.S | re.M)
    result = dict(pairs)
    assert len(result) == len(pairs), 'duplicate serialized ID'
    return result


def height(text, width, size):
    font = ImageFont.truetype(str(FONT), size)
    lines = ['']
    for word in text.split():
        assert font.getlength(word) <= width, ('unbreakable word', word)
        candidate = (lines[-1] + ' ' + word).strip()
        if lines[-1] and font.getlength(candidate) > width:
            lines.append(word)
        else:
            lines[-1] = candidate
    return math.ceil(len(lines) * sum(font.getmetrics()) * 1.2)


def main():
    source = PREFAB.read_text()
    prefab = blocks(source)
    relative = PREFAB.relative_to(ROOT.parents[1])
    baseline = subprocess.check_output(['git', 'show', 'HEAD:' + str(relative)], cwd=ROOT.parents[1], text=True)
    assert set(prefab) == set(blocks(baseline))
    references = lambda text: re.findall(r'(?:fileID|guid): [\w-]+', text)
    assert references(source) == references(baseline), 'binding identity changed'
    assert '_hideChoiceText: 0' in source and '_placeChoicesHorizontally: 0' in source
    assert 'm_SizeDelta: {x: 380, y: 88}' in prefab['244066404829944102']
    assert 'm_SizeDelta: {x: -40, y: -20}' in prefab['6892249449337105610']
    assert 'm_AnchoredPosition: {x: 0, y: 210}' in prefab['8366115894970273688']
    assert 'm_SizeDelta: {x: 360,' in prefab['7768295843298073233']
    for ident in ['7454603924991342091', '5238556610448275617', '601236819358430851', '371307126571125144', '5874689758174330343']:
        assert 'm_FontSize: 22' in prefab[ident]
        assert 'm_Color: {r: 0.94, g: 0.92, b: 0.86, a: 1}' in prefab[ident]
    assert 'm_FontSize: 20' in prefab['3738310639071766514']

    # Observe the existing exhaustive source walker without changing Ink or its
    # semantic variables. An auxiliary last-line key follows its deep-copied state.
    spec = importlib.util.spec_from_file_location('tk_source_check', ROOT / 'Art/check_story.py')
    checker = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(checker)
    contexts, dialogue = [], set()

    def observe(frame, event, arg):
        if event != 'call' or frame.f_code.co_name != 'walk' or frame.f_code.co_filename != str(ROOT / 'Art/check_story.py'):
            return
        todo, state = frame.f_locals['todo'], frame.f_locals['state']
        if not todo:
            return
        node = todo[0]
        if node[0] == 'text':
            speaker, body = node[3].split(':', 1)
            if speaker.strip() not in ['Локация', 'Музыка', 'Звук']:
                state['_ui_previous'] = (speaker.strip(), body.strip())
                dialogue.add((speaker.strip(), body.strip()))
        elif node[0] == 'choice':
            available = [o for o in node[1] if checker.expression(o[1], state)]
            assert '_ui_previous' in state
            contexts.append((state['_ui_previous'], tuple(o[2] for o in available)))

    try:
        sys.setprofile(observe)
        with contextlib.redirect_stdout(io.StringIO()):
            checker.main()
    finally:
        sys.setprofile(None)
    assert contexts and max(len(labels) for _, labels in contexts) == 3
    labels = {label for _, group in contexts for label in group}
    assert len(labels) == 13
    # Expand CanvasScaler means logical height >=1024. Derive top using actual
    # viewport, narrator/named root offsets and text anchoredPosition.y=-40.
    top = lambda speaker: 1024 / 2 - 210 + (90 if speaker == '...' else 115) + 40
    width = lambda speaker: 360 if speaker == '...' else 380
    body_bottom = max(top(s) + height(t, width(s), 22) + 40 for s, t in dialogue)
    choice_bottom = max(top(s) + height(t, width(s), 22) + len(group) * (88 + 12)
                        for (s, t), group in contexts)
    assert body_bottom < 976, ('body bottom', body_bottom)
    assert choice_bottom < 976, ('choice bottom', choice_bottom)
    max_label = max(height(t, 340, 20) for t in labels)
    assert max_label <= 68, ('label box height', max_label)
    image_evidence = []
    for name in ['dialogue-panel.png', 'choice-card.png']:
        path = ROOT / 'Assets/Presentation/bubble/sprites' / name
        assert hashlib.sha256(path.read_bytes()).hexdigest() == HASH
        im = Image.open(path)
        assert im.mode == 'RGBA' and im.getchannel('A').getextrema() == (0, 255)
        # Live text stays in the quiet centre. Measure every source pixel there,
        # including opacity, against intended text color; no compositing/editing.
        region = im.crop((220, 170, 1952, 554))
        alpha_min = region.getchannel('A').getextrema()[0]
        assert alpha_min >= 250, ('reading field opacity', alpha_min)
        def luminance(rgb):
            c = [v/255 for v in rgb]
            c = [v/12.92 if v <= .04045 else ((v+.055)/1.055)**2.4 for v in c]
            return sum(v*w for v,w in zip(c,[.2126,.7152,.0722]))
        # Include source alpha over pure white (worst background for light text).
        light = max(luminance(tuple(v*a/255 + 255-a for v in rgb))
                    for *rgb, a in set(region.get_flattened_data()))
        contrast = (luminance((240,235,219)) + .05) / (light + .05)
        assert contrast >= 7, ('text contrast', contrast)
        image_evidence.append({'name':name,'size':im.size,'text_alpha_min':alpha_min,
                               'min_contrast_over_white':round(contrast,2)})
    print(json.dumps({'scope':'static estimate only; Unity/safe-area/face/pressed-state pending',
                      'objects':len(prefab),'references':len(references(source)),
                      'dialogue_strings':len(dialogue),'choice_contexts':len(contexts),'labels':len(labels),
                      'max_label_height':max_label,'body_bottom':body_bottom,'choice_bottom':choice_bottom,
                      'bottom_reserve':1024-max(body_bottom,choice_bottom),'images':image_evidence},indent=2))


if __name__ == '__main__':
    main()
    if '--self-test' in sys.argv:
        from unittest.mock import patch
        original = Path.read_text
        mutations = [('_hideChoiceText: 0', '_hideChoiceText: 1'),
                     ('_placeChoicesHorizontally: 0', '_placeChoicesHorizontally: 1'),
                     ('m_SizeDelta: {x: 380, y: 88}', 'm_SizeDelta: {x: 184, y: 160}'),
                     ('m_AnchoredPosition: {x: 0, y: 210}', 'm_AnchoredPosition: {x: 0, y: -50}'),
                     ('guid: 3fc21402c0e740339af097f931be7960', 'guid: 00000000000000000000000000000000')]
        for before, after in mutations:
            def mutated(path, *args, **kwargs):
                text = original(path, *args, **kwargs)
                return text.replace(before, after) if path == PREFAB else text
            with patch.object(Path, 'read_text', mutated), contextlib.redirect_stdout(io.StringIO()):
                try:
                    main()
                except AssertionError:
                    pass
                else:
                    raise RuntimeError('Regression not detected: ' + before)
        print('Five presentation regression fixtures rejected; no files changed.')
