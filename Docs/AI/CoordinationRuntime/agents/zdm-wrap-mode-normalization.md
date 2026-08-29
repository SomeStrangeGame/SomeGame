# Agent: zdm-wrap-mode-normalization

- Status: completed
- Task: привести ZDM character PNG wrap mode к эталону TZM и удалить 25 ставших безопасными exact duplicates через aliases.
- Scope: 194 ZDM character PNG meta, zdm.asset, character trim manifest, 25 alias-source PNG/meta, empty folder meta, собственные coordination-файлы.
- Expected files: ZDM character `.png.meta`, definition asset, trim manifest, 25 duplicate PNG/meta, coordination files.
- Constraints: не менять Ink/TZM importer settings/SDK; менять в meta только wrapU/V/W; Unity не запускать; команды последовательно.
- Started UTC: 2026-08-28T08:46:13Z
- Lock acquired UTC: 2026-08-28T08:46:30Z
- Heartbeat UTC: 2026-08-28T09:03:44Z
- Completed UTC: 2026-08-28T09:03:44Z
- Result: ZDM story PNG приведены к TZM Clamp; последние 25 exact duplicates
  переведены на aliases и удалены; static audits, doctor, diff check и Ink
  hash check успешны.
