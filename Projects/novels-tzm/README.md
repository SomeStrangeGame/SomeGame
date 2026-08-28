# TZM content

Unity-проект полного контента истории `tzm`. Результат сборки — один Story AssetBundle и release-метаданные для каждой целевой платформы.

Проект хранит ровно одну историю, поэтому её ID не повторяется в физических
папках:

```text
Assets/
  tzm.asset
  Ink/
  Characters/
  Locations/
  Choices/
  Presentation/
  Video/
```

`tzm.asset` — единая точка настройки runtime истории. `Config/card.json`
остаётся отдельной публичной карточкой публикации. Старые технические адреса
`content/tzm/**`, `noveltexts/tzm/**` и `novelsvideos/tzm/**` формируются
сборщиком и не должны воспроизводиться в структуре проекта вручную.
