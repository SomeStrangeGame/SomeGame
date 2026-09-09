"""Read-only source/release/APK identity check; no Unity, ADB or runtime claim."""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

STORY = Path(__file__).resolve().parents[1]
ROOT = STORY.parents[1]
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def verify():
    plan = json.loads((STORY/'Art/ACCEPTANCE_ROUTES.json').read_text())
    assert sha(STORY/'Assets/Ink/s01e01.ink') == plan['sourceSha256']
    report = json.loads((STORY/'Art/EdgeCleanup/v6/report.json').read_text())
    for row in report['files']:
        png = STORY/'Assets/Characters'/row['path']
        assert sha(png) == row['afterSha256'], png
        assert sha(png.with_suffix('.png.meta')) == row['metaSha256'], png
    compiled = STORY/'Assets/Ink/chernaya-melnitsa.ink.json'
    assert json.loads(compiled.read_text())['inkVersion'] > 0
    source_map = compiled.with_name(compiled.name+'.source-map.json')
    entries = json.loads(source_map.read_text())['_entries']
    assert entries and all(row['FileName'] == 's01e01.ink' for row in entries)
    assert all(1 <= row['LineNumber'] <= len((STORY/'Assets/Ink/s01e01.ink').read_text().splitlines()) for row in entries)
    assert json.loads((ROOT/'Projects/novels-catalog/Config/catalog.json').read_text())['stories'] == ['chernaya-melnitsa']
    local = ROOT/'Novels/Build/LocalContent'
    release_path = 'stories/chernaya-melnitsa/Remote/Android/release.json'
    release = json.loads((local/release_path).read_text())
    assert release['releaseId'] == 'fb7c51d4b322ccbee639cb642f7fa3c1cbb7019affe495ce2e355e38d2c3e267'
    apk = ROOT/'Novels/Build/Players/chernaya-melnitsa-v6-20260908/Novels.apk'
    apk_hash = sha(apk)
    assert apk_hash == '39049f824adf96c9f0b65a3b17671520f9853bb3fddaba52f83858dd5e456f01'
    prefix = 'assets/NovelContent/'
    with zipfile.ZipFile(apk) as package:
        assert json.loads(package.read(prefix+'catalog/registry/catalog.json'))['stories'] == ['chernaya-melnitsa']
        ids = {p[len(prefix+'stories/'):].split('/')[0] for p in package.namelist() if p.startswith(prefix+'stories/')}
        assert ids == {'chernaya-melnitsa'}
        platforms = {p.split('/Remote/',1)[1].split('/')[0] for p in package.namelist()
                     if p.startswith(prefix) and '/Remote/' in p}
        assert platforms == {'Android'}, platforms
        assert package.read(prefix+release_path) == (local/release_path).read_bytes()
        for bundle in release['bundles']:
            path = 'stories/chernaya-melnitsa/Remote/Android/'+bundle['name']+'/'+bundle['version']
            assert sha(local/path) == bundle['sha256']
            assert hashlib.sha256(package.read(prefix+path)).hexdigest() == bundle['sha256']
        for item in release['files']:
            path = 'stories/chernaya-melnitsa/'+item['payloadPath']
            assert sha(local/path) == item['sha256']
            assert hashlib.sha256(package.read(prefix+path)).hexdigest() == item['sha256']
    return {'status':'static-identity-pass-runtime-still-pending',
            'sourceSha256':plan['sourceSha256'], 'compiledSha256':sha(compiled),
            'sourceMapSha256':sha(source_map),'sourceMapEntries':len(entries),
            'verifiedCharacterPngsAndMetas':len(report['files']),
            'apk':str(apk.relative_to(ROOT)), 'apkSha256':apk_hash,
            'releaseId':release['releaseId'], 'verifiedBundles':len(release['bundles']),
            'verifiedPayloadFiles':len(release['files']), 'catalogStories':['chernaya-melnitsa'],
            'runtimeExecuted':False, 'originalityEvidence':'Existing four scoped passes retained; no new review'}

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path)
    args = parser.parse_args()
    result = verify()
    data = json.dumps(result,ensure_ascii=False,indent=2)+'\n'
    if args.output:
        args.output.write_text(data)
    print(data)
