# Agent: gpl-character-layers

- Status: yielded
- Task: подготовить Lea/Mark/Vera layers из последнего утверждённого состава с локальным детерминированным alpha/masking.
- Scope: GPL Characters, definition/Ink only if required, own coordination files.
- Expected files: common-canvas base, hair front/back, station clothes, required episode emotions/accessories, reference/recomposed proofs and Unity meta.
- Base commit: `7e9c7727`.
- Resume: пользователь разрешил CLI/API image editing; новая FIFO-заявка `20260828T163705Z-gpl-character-layers` ожидает после `official-unity-mcp`, runtime write-lock не удерживается.
- Lock acquired UTC: 2026-08-28T16:42:22Z.
- Blocked: CLI dry-run и edit-mask проверены, но `OPENAI_API_KEY` и пакет `openai` недоступны текущему процессу Codex; runtime lock освобождён до появления доступа.
- Resume: пользователь подтвердил продолжение через встроенный imagegen без API-ключа; новая заявка `20260828T164849Z-gpl-character-layers`, lock получен.
- Result: три встроенные итерации основы и детерминированная reverse-composite проверка не прошли registration gate; импорт отсутствует. Для корректного набора нужен новый base-first master и производные слои, а не обратное восстановление скрытого тела из baked-персонажа.
- Resume: создаётся новый base-first master; голова уменьшается относительно отклонённой основы, исходный baked-персонаж используется только как identity/style reference.
- Result: новый base-first master с уменьшенной головой создан; изолированная причёска зарегистрирована детерминированно на общем холсте `1024x1536`, target bbox `430,108–575,300`; ожидается визуальное утверждение.
- Correction: пользователь восстановил канонический baked-master-first процесс; последние base-first/hair-generation варианты отклонены. Выполняется только exact-pixel alpha extraction из одного цельного master без resize/shift.
- Result: из утверждённого цельного master получены exact-pixel hair и clothes alpha-слои на неизменном холсте `1024x1536`; skin leakage устранён, показан proof; импорт ожидает approval.
