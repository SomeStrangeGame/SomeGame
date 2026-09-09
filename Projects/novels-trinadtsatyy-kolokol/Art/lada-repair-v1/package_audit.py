"""Source-only Lada geometry/alpha/provenance checks and user-approved proofs.

No Unity, production writes, facial editing or sprite registration changes.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path
import unittest

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
STORY = ROOT.parent.parent
RUNTIME = STORY / 'Assets/Characters/maincharacter/view/whole/coat'
NAMES = ('main', 'focused', 'alarmed', 'resolve')
FILES = {'main': 'main-clean-v2.png', **{n: n+'-clean.png' for n in NAMES[1:]}}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def geometry(im):
    assert im.mode == 'RGBA', 'Actual RGBA required'
    assert im.size == (1254, 1254), 'Candidate canvas/quality regression'
    a = np.asarray(im)
    alpha = a[..., 3]
    assert not alpha[:2].any() and not alpha[-2:].any(), 'Top/bottom residue'
    assert not alpha[:, :2].any() and not alpha[:, -2:].any(), 'Side residue'
    assert np.max(np.abs(np.array(im.getbbox()) - (430, 13, 868, 1213))) <= 2, 'Registration drift'
    white = (a[..., :3] > 200).all(axis=2) & (alpha > 240)
    assert not (white.sum(axis=1) > im.width * .5).any(), 'Opaque pale stripe'
    return alpha > 128


def audit(verify_import=False):
    ims = {n: Image.open(ROOT/FILES[n]) for n in NAMES}
    reference = geometry(ims['main'])
    records = {}
    for name, im in ims.items():
        mask = geometry(im)
        overlap = float((mask & reference).sum() / (mask | reference).sum())
        assert overlap > .98, (name, 'Silhouette drift', overlap)
        meta = RUNTIME/(name+'.png.meta')
        guid = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(), re.M)
        assert guid, meta
        records[name] = dict(file=FILES[name], sha256=sha(ROOT/FILES[name]),
            bounds=list(im.getbbox()), silhouetteIoU=overlap,
            metaSha256=sha(meta), guid=guid[1], target=str((RUNTIME/(name+'.png')).relative_to(STORY)))
    refs = []
    for src in sorted((STORY/'Assets/Ink').glob('s01e*.ink')):
        for line_no, line in enumerate(src.read_text().splitlines(), 1):
            match = re.match(r'^\s*Лада(?: \(([^)]+)\))?:', line)
            if match:
                emotion = match[1] or 'main'
                assert emotion in NAMES, (src, line_no, emotion)
                refs.append(dict(scene=src.name, line=line_no, selector='Лада',
                    outfit='coat', emotion=emotion,
                    address='maincharacter/view/whole/coat/'+emotion,
                    file=records[emotion]['target']))
    assert {r['emotion'] for r in refs} == set(NAMES)
    assert len({r['scene'] for r in refs}) == 6
    report = dict(status='source-static-pass-not-runtime-acceptance', records=records, sceneReferences=refs)
    if verify_import:
        before = json.loads((ROOT/'package-before-import.json').read_text())
        for name in NAMES:
            assert records[name]['metaSha256'] == before['records'][name]['metaSha256'], 'Meta modified'
            assert sha(RUNTIME/(name+'.png')) == records[name]['sha256'], 'Wrong imported source'
            geometry(Image.open(RUNTIME/(name+'.png')))
        print(json.dumps({'result':'PASS', 'files':4, 'sceneReferences':len(refs), 'metasUnchanged':True}))
        return
    destination = ROOT/'package-before-import.json'
    assert not destination.exists(), 'Do not overwrite baseline evidence'
    destination.write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n')
    # Proof-only resizing/cropping; production sprites remain untouched.
    for label, bg in [('light','#ffffff'), ('dark','#101016')]:
        sheet = Image.new('RGB', (1504, 408), bg)
        draw = ImageDraw.Draw(sheet)
        for i, name in enumerate(NAMES):
            composed = Image.new('RGBA', ims[name].size, bg)
            composed.alpha_composite(ims[name])
            sheet.paste(composed.convert('RGB').resize((376,376), Image.Resampling.LANCZOS), (i*376,28))
            draw.text((i*376+12,8), name, fill='#808080')
        sheet.save(ROOT/('package-full-'+label+'.png'))
    face = Image.new('RGB',(1200,340),'#24242a')
    draw = ImageDraw.Draw(face)
    for i, name in enumerate(NAMES):
        composed=Image.new('RGBA',ims[name].size,'#24242a'); composed.alpha_composite(ims[name])
        face.paste(composed.crop((535,8,785,258)).convert('RGB').resize((300,300),Image.Resampling.LANCZOS),(i*300,30))
        draw.text((i*300+10,8),name,fill='white')
    face.save(ROOT/'package-faces.png')
    print(json.dumps({'result':'PASS', 'files':4, 'sceneReferences':len(refs), 'IoU':{n:records[n]['silhouetteIoU'] for n in NAMES}}))


class Tests(unittest.TestCase):
    def test_good(self): geometry(Image.open(ROOT/FILES['main']))
    def test_low_resolution(self):
        with self.assertRaises(AssertionError): geometry(Image.new('RGBA',(620,620)))
    def test_rgb(self):
        with self.assertRaises(AssertionError): geometry(Image.new('RGB',(1254,1254)))
    def test_stripe(self):
        im=Image.open(ROOT/FILES['main']).copy()
        ImageDraw.Draw(im).line((0,1240,1253,1240),fill='white',width=1)
        with self.assertRaises(AssertionError): geometry(im)
    def test_shift(self):
        im=Image.new('RGBA',(1254,1254)); im.alpha_composite(Image.open(ROOT/FILES['main']),(12,0))
        with self.assertRaises(AssertionError): geometry(im)


if __name__=='__main__':
    parser=argparse.ArgumentParser(); parser.add_argument('--self-test',action='store_true'); parser.add_argument('--verify-import',action='store_true')
    args=parser.parse_args()
    if args.self_test:
        result=unittest.TextTestRunner().run(unittest.defaultTestLoader.loadTestsFromTestCase(Tests))
        raise SystemExit(0 if result.wasSuccessful() else 1)
    audit(args.verify_import)
