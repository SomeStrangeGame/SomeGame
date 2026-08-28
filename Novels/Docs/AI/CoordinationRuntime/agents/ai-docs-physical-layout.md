# Agent: ai-docs-physical-layout

- Status: completed
- Task: физически структурировать `Novels/Docs/AI` и обновить ссылки
- Scope:
  - `AGENTS.md`
  - `Novels/Docs/AI/**/*.md`, кроме чужих runtime agent/request-записей
  - собственные coordination files
- Expected result: rules, guides, architecture, plans и archive разнесены по каталогам; битых ссылок и старых путей нет.

- Validation: 38 документов распределены по каталогам; 0 битых Markdown-ссылок; 0 старых активных путей вне исторических runtime-записей; scoped `git diff --check` успешен.
