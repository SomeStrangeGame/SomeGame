# Agent: texture-compression-protocol

- Status: completed
- Task: закрепить обязательный texture-compression contract без дублирования экспериментального size-плана.
- Scope: `ContentAuthoringGuide.md`, own coordination files.
- Base commit: `7e9c7727`.

## Result

- В канонический authoring guide добавлен обязательный texture-compression
  contract: единый postprocessor, ASTC 8×8, importer flags, Max Size, alpha,
  размеры вне block multiple, reimport/versioning, последовательные builds,
  измерение bundle/GPU memory и visual gate.
- Экспериментальный size-план оставлен справочным и не дублирует контракт.
