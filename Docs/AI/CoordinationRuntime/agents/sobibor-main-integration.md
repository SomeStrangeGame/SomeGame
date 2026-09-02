# Agent: sobibor-main-integration

- Status: completed
- Task: перенести проверенный атомарный проект Sobibor из Codex worktree в канонический checkout `main`, сохранив существующий dirty tree.
- Scope: `Projects/novels-sobibor/**`, точечное объединение `Projects/novels-catalog/Config/catalog.json`, собственные coordination records; shared runtime и прочие dirty paths не меняются.
- Source: `/Users/iantonishin/.codex/worktrees/cb93/SomeGame`, commit base `69d77aa9c04d6b104acd5cea32c074b2778802ff`.
- Validation: byte parity source/target, catalog JSON merge, Ink traversal, targeted Sobibor/catalog content gates when the shared Unity resource is available, scoped diff review.
- Result: exact authored atomic project copied into canonical `main`; all current catalog registrations preserved and `sobibor` appended.
- Canonical validation: source parity before build clean; card/catalog JSON passed; `novels-content doctor`, `validate sobibor`, `validate catalog`, `build sobibor editor`, and `build catalog editor` passed. Release `f223f865f1415833057ddfa2b196621d35a244ba465fcbc177a81b340bc17cb4` contains 3 chunks, 6 locations, and 36 character assets.
- Finished UTC: `2026-09-02T16:54:00Z`.
- Limitation: no additional manual Play Mode visual replay; remote push not requested and not performed.
