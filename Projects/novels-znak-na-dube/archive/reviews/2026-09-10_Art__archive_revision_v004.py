"""Preserve dated, non-overwriting story checkpoints; stdlib only, no Unity.

Text is copied byte-for-byte. Large existing binaries remain local with verified
pointers; this does not claim durable remote retention or historical completeness.
"""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path
import sys

PROJECT = Path(__file__).resolve().parents[1]
ARCHIVE = PROJECT / 'archive'
GENERATED = Path('/Users/iantonishin/.codex/generated_images/01a07c22-8e29-7343-8148-1241fc75279e')
OLD_COVERS = {
    'exec-b46513cf-d5cf-4d1d-86b2-ce6d8302ce3a.png': 'rejected-symbol',
    'exec-d5ab7a77-2dab-40e0-99be-2b124b6f85fd.png': 'rejected-similarity',
    'exec-7fa7edc7-a67c-4e70-9f84-3ac103672ae6.png': 'selected-bell',
}


def digest(data):
    return hashlib.sha256(data).hexdigest()


def snapshot(stage):
    now = datetime.now(timezone.utc)
    stamp, day = now.isoformat(), now.strftime('%Y-%m-%d')
    previous = sorted(ARCHIVE.glob('*_manifest_v*.json')) if ARCHIVE.exists() else []
    version = len(previous) + 1
    suffix = f'v{version:03d}'
    records, gaps = [], []
    sources = sorted(set(PROJECT.glob('Art/*.md')) | set(PROJECT.glob('Art/*.ink')) |
                     set(PROJECT.glob('Assets/Ink/*.ink')) | set(PROJECT.glob('Config/*.json')) |
                     set(PROJECT.glob('Config/Preview/*.json')) | set(PROJECT.glob('Art/*.py')) |
                     {PROJECT / 'README.md'})
    for src in sources:
        rel = src.relative_to(PROJECT).as_posix()
        category = 'scenarios' if src.suffix == '.ink' or 'SCENARIO' in src.name or 'NARRATIVE' in src.name else 'decisions'
        if 'VALIDATION' in src.name or 'ORIGINALITY' in src.name or src.suffix == '.py':
            category = 'reviews'
        name = rel.replace('/', '__')
        dst = ARCHIVE / category / f'{day}_{name.rsplit(".", 1)[0]}_{suffix}{src.suffix}'
        dst.parent.mkdir(parents=True, exist_ok=True)
        data = src.read_bytes()
        with dst.open('xb') as output:
            output.write(data)
        assert dst.read_bytes() == data
        records.append({'id': rel, 'stage': stage, 'category': category,
                        'path': dst.relative_to(ARCHIVE).as_posix(), 'format': src.suffix[1:],
                        'sha256': digest(data), 'bytes': len(data), 'snapshotUtc': stamp,
                        'sourceDate': 'unknown', 'source': rel, 'creatorRole': 'AI-assisted project material',
                        'toolModel': 'unknown', 'status': 'baseline' if version == 1 else 'checkpoint',
                        'approval': 'User authorized revision in auto-approve mode; not explicit artifact acceptance',
                        'productionMapping': rel, 'parentManifest': previous[-1].name if previous else None,
                        'decision': 'See Art/REVISION_HANDOFF.md and Art/REVISION_BRIEF.md snapshots',
                        'availability': 'retained bytes', 'redactions': [], 'gaps': []})
    binaries = list(PROJECT.glob('Assets/**/*.png')) + list(PROJECT.glob('Assets/**/*.wav')) + list(PROJECT.glob('Config/**/*.png'))
    binaries += [GENERATED / name for name in OLD_COVERS]
    for src in sorted(binaries):
        if not src.is_file():
            gaps.append(str(src))
            continue
        data = src.read_bytes()
        records.append({'id': str(src), 'stage': stage, 'category': 'assets', 'externalPath': str(src),
                        'format': src.suffix[1:], 'sha256': digest(data), 'bytes': len(data),
                        'snapshotUtc': stamp, 'sourceDate': 'unknown', 'creatorRole': 'AI-generated project art',
                        'toolModel': 'unknown', 'status': OLD_COVERS.get(src.name, 'existing-production'),
                        'productionMapping': 'Config/EpisodeCovers/s01e01.png' if src.name in OLD_COVERS else str(src),
                        'decision': 'Art/EPISODE_COVER_HANDOFF.md for cover requests, inputs and rejections; other historical inputs incomplete',
                        'availability': 'verified local only; no remote retention guarantee',
                        'gaps': ['Earlier generation metadata may be unavailable']})
    result = {'storyId': 'znak-na-dube', 'version': version, 'stage': stage, 'recordedUtc': stamp,
              'predecessor': previous[-1].name if previous else None,
              'predecessorSha256': digest(previous[-1].read_bytes()) if previous else None,
              'archiveAdoption': '2026-09-10; old originals are dated by actual archival time',
              'knownGaps': ['Pre-adoption intermediate text/art drafts and original approval timestamps are not fully retained. No reconstruction is claimed.',
                            'Large binary retention is local only; no LFS/service configured.'] + gaps,
              'artifacts': records}
    ARCHIVE.mkdir(parents=True, exist_ok=True)
    target = ARCHIVE / f'{day}_manifest_{suffix}.json'
    with target.open('x', encoding='utf-8') as output:
        json.dump(result, output, ensure_ascii=False, indent=2)
        output.write('\n')
    verify(target)
    print(json.dumps({'manifest': str(target), 'sha256': digest(target.read_bytes()), 'records': len(records), 'gaps': result['knownGaps']}, ensure_ascii=False))


def verify(manifest):
    obj = json.loads(manifest.read_text())
    if obj['predecessor']:
        assert digest((ARCHIVE / obj['predecessor']).read_bytes()) == obj['predecessorSha256']
    for item in obj['artifacts']:
        path = ARCHIVE / item['path'] if 'path' in item else Path(item['externalPath'])
        assert path.is_file() and digest(path.read_bytes()) == item['sha256'], path
    return obj


if __name__ == '__main__':
    if sys.argv[1] == 'verify':
        target = sorted(ARCHIVE.glob('*_manifest_v*.json'))[-1]
        verify(target)
        print('Archive latest snapshot: passed')
    else:
        snapshot(sys.argv[1])
