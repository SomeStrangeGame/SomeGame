# Parallel work: video-camera-without-poster-fix

- Статус: ready-with-limitations
- Ветка: experiment/story-preview-streaming
- Базовый commit: d8853bf96f0c80d1e52c34e3ece8705cbc7018ac
- Ответственный поток: текущий чат — падение camera action на видеолокации без постера
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Location/View/LocationLayout.cs`
- `Packages/NovelsContentSdk/Runtime/Features/Location/View/LocationScreen.cs`
- `Novels/Docs/AI/ParallelWork.video-camera-without-poster-fix.md`
- собственные записи `Novels/Docs/AI/CoordinationRuntime/**`

## Не изменять

- `Projects/novels-*/Assets/Ink/**`
- story assets, prefabs и bundles
- остальные пользовательские изменения рабочего дерева

## Изменённые контракты

- Background layout принимает геометрию и Sprite, и RenderTexture видео.
- Camera/dialogue motion работает по общему visual container.

## Выполнено

- Runtime подтвердил успешный video crossfade и последующее падение первой
  команды `Камера: слева направо` из-за отсутствующего poster sprite.
- Video texture теперь конфигурирует ширину visual container по собственному
  aspect ratio; camera travel вычисляется по фактическому RectTransform.
- Dialogue alignment использует тот же общий признак настроенного visual.

## Проверено

- Unity Roslyn по актуальному `Novels.Location.rsp` — успешно.
- Открытый Unity Editor PID 29694 выполнил refresh, скомпилировал
  `Novels.Location.dll` и завершил Domain Reload без новых C#-ошибок.
- `причал.mp4`: 2160×1920; layout получает реальные размеры RenderTexture.
- Scoped `git diff --check` — успешно.

## Требуется при интеграции

- Повторный Play Mode smoke `Гардероб -> Причал -> камера`; автоматического
  управления Play Mode нет.
