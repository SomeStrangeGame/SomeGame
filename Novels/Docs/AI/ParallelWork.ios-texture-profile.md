# Parallel work: ios-texture-profile

- Статус: ready-for-integration
- Ветка: grandChange
- Базовый commit: c6c7853b
- Ответственный поток: ios-texture-profile
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`
- iOS generated releases TZM и ZDM
- size/status документация и собственные runtime coordination files

## Изменённый контракт

- Android сохраняет ASTC 6×6.
- iOS использует ASTC 8×8.
- Max Size 4096, mipmaps off и quality 100 не изменились.

## Проверено

- `novels-content build zdm ios` — успешно; bundle 66 039 336 B против
  baseline 116 864 846 B (−43,5%).
- `novels-content build tzm ios` — успешно; bundle 181 345 930 B против
  baseline 212 013 875 B (−14,5%).
- Регрессия TZM ASTC 6×6 до 305 236 981 B устранена.
- `git diff --check` — успешно.

## Требуется при интеграции

- Ручной visual quality gate на iOS, особенно лица, волосы, градиенты и UI.
