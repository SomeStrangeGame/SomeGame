# План упрощения Novels

Этот файл содержит только актуальную работу. Завершённые архитектурные волны
описаны в [RefactoringHistory.md](RefactoringHistory.md).

## Цель

Новый разработчик должен понимать запуск приложения, прочитав основной маршрут:

```text
EntryPoint
  -> ApplicationRuntime
  -> CatalogFlow
  -> NovelRuntime
  -> EpisodeRuntime
  -> StoryQueueBuilder
  -> StoryOperationExecutor
```

Подробная карта ответственности находится в
[ProjectOverview.md](ProjectOverview.md), а сборка контента — в
[ContentPipeline.md](ContentPipeline.md).

## Текущая волна

- [x] Убрать искусственную state machine из `ApplicationRuntime`.
- [x] Сделать загруженный каталог предметным lifetime-объектом.
- [x] Отделить replay validation от композиции эпизода.
- [x] Сократить повторяющийся поиск слоёв персонажа.
- [x] Централизовать построение однотипных адресов.
- [x] Сделать результат инспекции контентного проекта неизменяемым.
- [x] Удалить отдельные Bubble/Choose/Wardrobe contract assemblies, сохранив
  независимые Choose и Wardrobe features.
- [x] Убрать дублирование lifecycle между Choose и Wardrobe через композицию.
- [x] Подтвердить компиляцию Game и Content SDK.
- [x] Последовательно провалидировать Catalog, TZM и ZDM.
- [x] Пересобрать Editor bundles и локальную композицию.
- [ ] Проверить запуск Novels вручную в Editor.

## Вторая волна упрощения

- [x] Сделать validation линейным без одноразовых rule-классов.
- [x] Удалить `NovelBootstrapProcess` и `NovelStartSession`.
- [x] Удалить тривиальный `NovelSession`.
- [x] Заменить простые классы StoryOperation на `DelegateStoryOperation`.
- [x] Перенести статическую разметку OptionList в общий prefab без изменения
  размеров.
- [x] Заменить одно- и двухполевые `Dependencies` прямыми конструкторами в
  Choose, Wardrobe и Catalog.
- [x] Подтвердить Unity-компиляцию и validation Catalog/TZM/ZDM.
- [ ] Вручную проверить Choose и Wardrobe в Play Mode.

## Третья волна упрощения

- [x] Удалить неиспользуемый уровень warning из общего validation report.
- [x] Заменить промежуточный `ContentProject` компактным `ContentBuildPlan`.
- [x] Определять Catalog/Story по единственному JSON-маркеру проекта.
- [x] Оставить подробный состав bundle только в диагностике ошибки.
- [x] Вынести смысловую и визуальную приёмку в ручной чек-лист.
- [x] Подтвердить Unity-компиляцию, validation всех проектов и сборку Catalog.

## Не менять в этой волне

- Форматы Ink, release и save.
- Размеры и геймдизайн существующего UI.
- Разделение Choose и Wardrobe на самостоятельные фичи.
- Контент Catalog, TZM и ZDM, кроме артефактов штатной пересборки.
- Политику отсутствующего контента и fallback assets.

## Критерий завершения

1. Unity компилирует runtime и Editor assemblies без новых ошибок.
2. `novels-content doctor` проходит.
3. `validate catalog`, `validate tzm` и `validate zdm` проходят.
4. Editor bundles последовательно пересобраны общей версией SDK.
5. В рабочем дереве нет случайных Unity- или временных файлов.
6. Результат и ограничения записаны в coordination handoff.
