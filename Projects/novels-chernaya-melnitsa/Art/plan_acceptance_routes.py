"""Plan actual replays from audited Ink; never launches Unity or touches saves.

Coverage means each authored condition outcome, option, displayed source line
and ending, NOT all 72 combinations in a Player. Runtime evidence stays empty.
"""
import argparse
import contextlib
import hashlib
import io
import json
from pathlib import Path
import runpy


def build_plan():
    with contextlib.redirect_stdout(io.StringIO()):
        audit = runpy.run_path(str(Path(__file__).with_name('audit_story.py')))
    routes = audit['routes']

    def coverage(route):
        return ({f'condition:{n}:{side}' for n, side in route['conditions']}
                | {f'choice:{key}' for key in route['choices']}
                | {f'line:{n}' for n in route['lines']}
                | {f'ending:{route["state"]["ending"]}'})

    sets = [coverage(route) for route in routes]
    universe = set().union(*sets)
    remaining = universe.copy()
    selected = []
    while remaining:
        index = max(range(len(routes)), key=lambda i: (len(sets[i] & remaining), -i))
        assert sets[index] & remaining
        selected.append(index)
        remaining -= sets[index]
    # Remove redundant runs; deterministic greedy cover, no optimality claim.
    for index in selected[:]:
        others = [sets[i] for i in selected if i != index]
        if others and set().union(*others) == universe:
            selected.remove(index)
    assert set().union(*(sets[i] for i in selected)) == universe
    groups = audit['choice_groups']
    rows = []
    for number, index in enumerate(selected, 1):
        route = routes[index]
        rows.append({
            'id': f'R{number:02d}', 'enumeratedRouteIndex': index,
            'choices': [{'id': key, 'optionIndex': groups[line].index(key),
                         'sourceLine': line, 'expectedStateBefore': state}
                        for (line, _), key, state in zip(groups.items(), route['choices'], route['choice_states'])],
            'ending': route['state']['ending'], 'expectedFinalState': route['state'],
            'conditionOutcomes': route['conditions'], 'expectedSourceLines': route['lines'],
            'runtimeStatus': 'not-run', 'runtimeEvidence': []})
    return {'status': 'planned-not-executed',
            'sourceSha256': hashlib.sha256(audit['SOURCE'].read_bytes()).hexdigest(),
            'staticCombinations': len(routes), 'selectedRuns': len(rows),
            'selection': 'deterministic greedy set cover with redundancy removal; not a proven minimum',
            'coverage': {prefix: sum(x.startswith(prefix+':') for x in universe)
                         for prefix in ('condition', 'choice', 'line', 'ending')},
            'uncovered': [], 'routes': rows}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    plan = build_plan()
    if args.output:
        args.output.write_text(json.dumps(plan, ensure_ascii=False, indent=2)+'\n')
    print(json.dumps({key: value for key, value in plan.items() if key != 'routes'}, ensure_ascii=False))
    for route in plan['routes']:
        print(route['id'], ' / '.join(choice['id'] for choice in route['choices']), '=>', route['ending'])
