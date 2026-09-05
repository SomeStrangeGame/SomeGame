# Story-local Bubble presentation

Общий контракт для существенного redesign story-local Bubble UI. Жанровый
skill задаёт художественный профиль, аудиторию и специальные ограничения, а
этот guide — общие границы реализации и проверки.

## Границы

- Меняется presentation конкретной истории, а не смысл текста, topology
  выборов, shared runtime или neutral fallback.
- Story-local prefab сохраняет действующий Bubble runtime contract и fallback
  для отсутствующих изображений.
- Текст остаётся доступным даже при image-led выборе; иллюстрация не должна
  уничтожать читаемость диалога, controls или safe areas.
- Новые raster assets создаются через `$imagegen` и проходят применимые art и
  originality-требования.

## Проверка

Проверить реальные dialogue, choice, long-text, missing-image, disabled/pressed
и целевые aspect-ratio состояния. Статический prefab audit не заменяет
визуальный просмотр, когда changed-path plan требует manual gate.

Handoff перечисляет prefab/assets, сохранённые runtime bindings, проверенные
состояния, visual evidence и незавершённые manual gates.
