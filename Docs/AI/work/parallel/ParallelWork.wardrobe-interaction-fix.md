# Parallel work: wardrobe-interaction-fix

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c7727
- Ответственный поток: исправление интерактивных вкладок и слоя персонажа fallback-гардероба
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/**`
- `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/**`
- `Packages/NovelsContentSdk/Runtime/Features/Character/**`
- минимальные `Novels/Assets/Novels/**` integration points
- собственные coordination-записи

## Не изменять

- Ink и compiled Ink
- content assets и texture profiles
- обычный Choose contract/behavior
- посторонние изменения рабочего дерева

## Изменённые контракты

- Вкладки просматривают категории из sprite-trim manifest без продвижения Ink.
- Подтверждение исходной сюжетной категории сохраняет прежний Ink-контракт;
  подтверждение вспомогательной категории возвращает к сюжетной категории.

## Проверено до изменений

- Вкладки созданы как `Image` с отключённым raycast и без listener.
- Backdrop является вложенным canvas верхнего OptionList canvas и перекрывает character canvas.

## Реализовано

- Wardrobe backdrop вынесен в отдельный root canvas между location и character,
  поэтому персонаж снова видим.
- Вкладки стали настоящими кнопками и загружают варианты лица, волос, одежды и
  аксессуаров из manifest текущей истории.
- Выбор варианта обновляет персонажа живым preview; обычный Choose и Ink не
  изменены.

## Проверка

- Unity 6000.3.11f1 batch compile: exit code 0, `Exiting batchmode successfully`.
- Licensing handshake успешен после штатного закрытия зависшего Hub и удаления
  только подтверждённого бесхозного нулевого `Novels/Temp/UnityLockfile`.
- Roslyn Unity compile не сообщил C# errors; scoped `git diff --check` успешен.
- Отдельный повторный `dotnet --no-restore` после Unity не является валидным:
  Unity очистил `Temp/obj/.../project.assets.json`; до batch-run полный dotnet
  build проходил с 0 errors.

## Осталось

- Ручной visual/input check в Play Mode на 1080×1920: видимость персонажа,
  переключение всех четырёх вкладок, preview и подтверждение сюжетного выбора.
