# Agent: tzm-exclude-unused-bundle

- Status: completed
- Task: исключить authoring-группу `Не используется` из обычного story bundle.
- Scope: `ContentPipeline.cs`, authoring guide, собственные coordination-файлы
  и игнорируемые generated build outputs/logs TZM.
- Expected files: `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`,
  `Novels/Docs/AI/ContentAuthoringGuide.md`, собственный status и append-only
  handoff.
- Constraints: не менять арт, import settings, `tzm.asset`, ZDM и чужие файлы;
  Unity/build запускать только под live write-lock и последовательно.
- Result: bundle-root filtering реализован; static audit и Editor compile
  успешны, полный Unity validate/build отложен из-за несовместимого Licensing
  Client.
