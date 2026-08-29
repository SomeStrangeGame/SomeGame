# Parallel work: gpl-character-layers

- Статус: yielded
- Ветка: main
- Базовый commit: 7e9c7727
- Ответственный поток: персонажные слои GPL episode 1
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Projects/novels-gpl/Assets/Characters/**`
- `Projects/novels-gpl/Assets/gpl.asset`
- `Projects/novels-gpl/Assets/Ink/**` только при необходимости адресации
- собственные coordination files

## Не изменять

- другие content-проекты
- shared SDK/pipeline
- посторонний dirty tree

## Контракт

- Использовать последний утверждённый цельный состав персонажей.
- Локальная детерминированная обработка разрешена пользователем только для alpha, masking и регистрации.
- Каждый набор требует настоящего alpha, общего холста и доказанной обратной сборки.
- CLI/API image editing разрешён пользователем; заявка ожидает после `official-unity-mcp`, runtime lock не удерживается.
- Runtime write-lock получен: 2026-08-28T16:42:22Z.
- CLI edit dry-run успешен; реальный вызов заблокирован отсутствием `OPENAI_API_KEY` и Python-пакета `openai` в доступном runtime. Lock и FIFO-заявка освобождаются на время внешнего ожидания.
- Работа возобновлена через встроенный imagegen без API-ключа; новый runtime lock получен.
- Built-in base-first correction проверена reverse composite: остаточные выступы кожи по рукам/ногам означают reject; игровые assets не импортированы.
- Новый проход: сначала утверждается пропорциональная bald-base Лея с уменьшенной головой, затем только производные слои.
- Preview base+hair собран без изменения холста; причёска локально зарегистрирована в bbox `x=430…575, y=108…300`. Импорт ожидает approval.
- Последний base-first preview отклонён. Актуальный proof построен baked-master-first: `master.png`, `hair.png`, `clothes.png` содержат только исходные master pixels на общем `1024x1536`; ожидается visual approval.
