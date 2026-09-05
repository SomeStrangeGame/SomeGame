# Story-local Bubble presentation

Общий контракт существенного redesign story-local Bubble UI. Жанровый skill
задаёт художественный профиль, аудиторию и специальные ограничения; этот guide
владеет общей реализацией, state matrix и evidence.

## Граница ответственности

- Меняется presentation одной истории, но не текст, choice topology, story facts,
  shared runtime или neutral fallback.
- До дизайна инспектируются story prefab, effective fallback, runtime component
  fields и один рабочий story-local пример.
- Сохраняются filenames, serialized fields, component types, hierarchy bindings,
  runtime addresses и безопасный label fallback.
- Shared-contract defect возвращается отдельным scope; story-local skill не
  компенсирует его изменением общего runtime.

## Layout и assets

- Диалог, имя, controls, characters, evidence objects и safe areas остаются
  читаемыми на целевых aspect ratios.
- Image-led выбор всё равно сохраняет доступный текст; text-led выбор не зависит
  от иконки для понимания.
- UI sprites не содержат встроенных слов, имеют настоящую прозрачность и
  совместимый hidden RGB у краёв. Nine-slicing используется только после
  проверки реального runtime-размера.
- Новые raster assets создаются через `$imagegen` и проходят применимые art,
  licensing и originality contracts.
- Motion не разрушает чтение, не создаёт опасное мигание и учитывает доступный
  reduced-motion режим.

## Обязательная state matrix

Проверить все реально используемые semantic states и минимум:

1. длинный narrator/no-character текст на самом светлом и тёмном фоне;
2. длинный named-character текст с персонажами в authored positions;
3. максимальную реальную группу выборов и самые длинные labels;
4. pressed/disabled состояния, если runtime их использует;
5. permitted missing-image/icon fallback;
6. самый узкий поддерживаемый portrait viewport и safe area;
7. сильнейшее motion/special состояние жанрового профиля.

Сначала проверяются serialization и addresses, затем целевой Player, когда его
требует changed-path plan. Проверка подтверждает contrast, wrapping, reading
order, tap areas, state transitions, alpha edges и отсутствие missing/pink/
fallback sprites. Prefab preview или успешная компиляция не заменяют visual gate.

## Исправление и handoff

Меняется минимальный ответственный слой: artwork для поверхности, import/render
settings для краёв, prefab для geometry, runtime binding только при доказанном
binding defect.

Handoff перечисляет prefab/assets, runtime bindings, semantic states, fallback,
motion/accessibility, material import settings, validation, visual evidence и
все состояния, не просмотренные в реальном Player. Этот workflow не меняет Ink,
не регистрирует историю в каталоге и не публикует результат.
