# Agent: tzm-art-aliases

- Status: completed
- Task: добавить story-level Art Aliases и удалить только безопасные exact-byte
  дубликаты TZM без изменения Ink.
- Scope: SDK runtime/editor address resolution and validation, узкая передача
  resolver в NovelRuntime, `tzm.asset`, trim manifest, 17 подтверждённых
  alias-source PNG/meta, authoring docs и собственные coordination-файлы.
- Expected files: `Packages/NovelsContentSdk/Runtime/**`,
  `Packages/NovelsContentSdk/Editor/**`, `Novels/Assets/Novels/**`,
  `Projects/novels-tzm/Assets/tzm.asset`, character trim manifest, безопасные
  duplicate paths, собственный status и append-only handoff.
- Constraints: не менять Ink/ZDM/неоднозначные пары; не запускать Unity,
  validate или build до исправления Licensing Client; команды последовательно.
- Result: 17 aliases и safe duplicate migration реализованы; static audit и
  Content/ContentAddressing build успешны, Unity checks отложены по ограничению
  пользователя.
