"""Preserve dated, non-overwriting story checkpoints; stdlib only, no Unity.

Text is copied byte-for-byte. Versioned story-local binaries use portable,
hash-verified pointers. Rejected generated candidates are copied into the archive
because their original generated-images location is not durable project storage.
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


def category_for(src):
    if src.name == 'REVISION_BRIEF.md':
        return 'brief'
    if src.name == 'RESEARCH_LEDGER.md':
        return 'research'
    if src.suffix == '.ink' or 'SCENARIO' in src.name or 'NARRATIVE' in src.name:
        return 'scenarios'
    if 'VALIDATION' in src.name or 'ORIGINALITY' in src.name or src.suffix == '.py':
        return 'reviews'
    return 'decisions'


def archive_candidate(src, day, suffix, artifact_id, status, production_mapping, stamp):
    data = src.read_bytes()
    retained = [path for path in sorted((ARCHIVE / 'assets').glob(f'*_{artifact_id}_v*{src.suffix}'))
                if digest(path.read_bytes()) == digest(data)] if (ARCHIVE / 'assets').exists() else []
    if retained:
        dst = retained[-1]
    else:
        dst = ARCHIVE / 'assets' / f'{day}_{artifact_id}_{suffix}{src.suffix}'
        dst.parent.mkdir(parents=True, exist_ok=True)
        with dst.open('xb') as output:
            output.write(data)
        assert dst.read_bytes() == data
    return {
        'artifactId': f'candidate:{artifact_id}', 'stage': 'ready-for-final-validation',
        'category': 'assets', 'path': dst.relative_to(ARCHIVE).as_posix(),
        'format': src.suffix[1:], 'sha256': digest(data), 'bytes': len(data),
        'snapshotUtc': stamp, 'sourceDate': 'unknown', 'originalName': src.name,
        'originalLocation': str(src), 'sourceReference': 'Art/EPISODE_COVER_HANDOFF.md',
        'creatorRole': 'AI-generated candidate', 'toolModel': 'built-in image generation; exact model unavailable',
        'status': status, 'approval': 'Selected/rejected as recorded in Art/EPISODE_COVER_HANDOFF.md',
        'productionMapping': production_mapping, 'decision': 'See Art/EPISODE_COVER_HANDOFF.md',
        'availability': 'retained bytes in story archive; reused by hash in later manifests', 'redactions': [],
        'gaps': ['Exact model/version metadata unavailable']
    }


def snapshot(stage):
    now = datetime.now(timezone.utc)
    stamp, day = now.isoformat(), now.strftime('%Y-%m-%d')
    previous = sorted(ARCHIVE.glob('*_manifest_v*.json')) if ARCHIVE.exists() else []
    version = max((int(path.stem.rsplit('v', 1)[1]) for path in previous), default=0) + 1
    suffix = f'v{version:03d}'
    records, gaps = [], []
    sources = sorted(set(PROJECT.glob('Art/*.md')) | set(PROJECT.glob('Art/*.ink')) |
                     set(PROJECT.glob('Assets/Ink/*.ink')) | set(PROJECT.glob('Config/*.json')) |
                     set(PROJECT.glob('Config/Preview/*.json')) | set(PROJECT.glob('Art/*.py')) |
                     {PROJECT / 'README.md'})
    for src in sources:
        rel = src.relative_to(PROJECT).as_posix()
        category = category_for(src)
        name = rel.replace('/', '__')
        dst = ARCHIVE / category / f'{day}_{name.rsplit(".", 1)[0]}_{suffix}{src.suffix}'
        dst.parent.mkdir(parents=True, exist_ok=True)
        data = src.read_bytes()
        with dst.open('xb') as output:
            output.write(data)
        assert dst.read_bytes() == data
        records.append({'artifactId': f'text:{rel}', 'stage': stage, 'category': category,
                        'path': dst.relative_to(ARCHIVE).as_posix(), 'format': src.suffix[1:],
                        'sha256': digest(data), 'bytes': len(data), 'snapshotUtc': stamp,
                        'sourceDate': 'unknown', 'originalName': src.name, 'originalLocation': rel,
                        'sourceReference': rel, 'creatorRole': 'AI-assisted project material',
                        'toolModel': 'unknown', 'status': 'baseline' if version == 1 else 'checkpoint',
                        'approval': 'User authorized revision in auto-approve mode; not explicit artifact acceptance',
                        'productionMapping': rel, 'parentManifest': previous[-1].name if previous else None,
                        'decision': 'See Art/REVISION_HANDOFF.md and Art/REVISION_BRIEF.md snapshots',
                        'availability': 'retained bytes', 'redactions': [], 'gaps': []})
    binaries = (list(PROJECT.glob('Art/Source/*.png')) + list(PROJECT.glob('Art/AudioStems/*.wav')) +
                list(PROJECT.glob('Assets/**/*.png')) + list(PROJECT.glob('Assets/**/*.wav')) +
                list(PROJECT.glob('Config/**/*.png')))
    for src in sorted(binaries):
        data = src.read_bytes()
        rel = src.relative_to(PROJECT).as_posix()
        records.append({'artifactId': f'asset:{rel}', 'stage': stage, 'category': 'assets', 'externalPath': rel,
                        'format': src.suffix[1:], 'sha256': digest(data), 'bytes': len(data),
                        'snapshotUtc': stamp, 'sourceDate': 'unknown', 'originalName': src.name,
                        'originalLocation': rel, 'sourceReference': 'Art/ART_MANIFEST.md',
                        'creatorRole': 'AI-generated project art', 'toolModel': 'unknown',
                        'status': 'existing-production', 'approval': 'See the applicable story-local handoff',
                        'productionMapping': rel,
                        'decision': 'Art/ART_MANIFEST.md and domain handoffs; historical inputs incomplete',
                        'availability': 'hash-verified in the versioned story project', 'redactions': [],
                        'gaps': ['Earlier generation metadata may be unavailable']})
    for name, status in OLD_COVERS.items():
        src = GENERATED / name
        if not src.is_file():
            gaps.append(f'Unavailable generated candidate: {src}')
            continue
        artifact_id = {'rejected-symbol': 'episode-cover-rejected-symbol',
                       'rejected-similarity': 'episode-cover-rejected-similarity',
                       'selected-bell': 'episode-cover-selected-bell'}[status]
        records.append(archive_candidate(src, day, suffix, artifact_id, status,
                                         'Config/EpisodeCovers/s01e01.png', stamp))
    result = {'storyId': 'znak-na-dube', 'version': version, 'stage': stage, 'recordedUtc': stamp,
              'predecessor': previous[-1].name if previous else None,
              'predecessorSha256': digest(previous[-1].read_bytes()) if previous else None,
              'archiveAdoption': '2026-09-10; old originals are dated by actual archival time',
              'knownGaps': ['Pre-adoption intermediate text/art drafts and original approval timestamps are not fully retained. No reconstruction is claimed.',
                            'Some earlier generation tool/model metadata remains unavailable.'] + gaps,
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
    ids = [item.get('artifactId', item.get('id')) for item in obj['artifacts']]
    assert all(ids), 'artifact without stable identifier'
    assert len(ids) == len(set(ids)), 'duplicate artifactId'
    for item in obj['artifacts']:
        if 'path' in item:
            path = ARCHIVE / item['path']
        else:
            external = Path(item['externalPath'])
            path = external if external.is_absolute() else PROJECT / external
        assert path.is_file() and digest(path.read_bytes()) == item['sha256'], path
    return obj


if __name__ == '__main__':
    if sys.argv[1] == 'verify':
        target = sorted(ARCHIVE.glob('*_manifest_v*.json'))[-1]
        verify(target)
        print('Archive latest snapshot: passed')
    else:
        snapshot(sys.argv[1])
