# Agent: android-astc8-license

- Status: completed
- Task: штатно восстановить Unity Licensing IPC, завершить последовательные TZM/ZDM Android builds и дополнить протокол автономным recovery-loop.
- Scope: licensing guide, Android ASTC size/status docs, own coordination files, generated build state.
- Base commit: `7e9c7727`.

## Result

- Подтверждён Hub Licensing Client PID 51721, отвергавший LocalIPC 1.18.
- Hub закрыт штатно; два точных stale socket без владельца удалены.
- License, Unity/Hub cache, `hosts` и пользовательские данные не изменялись.
- TZM и ZDM Android builds завершены успешно, повторного licensing-сбоя нет.
- Licensing guide дополнен обязательным автономным recovery-loop.
