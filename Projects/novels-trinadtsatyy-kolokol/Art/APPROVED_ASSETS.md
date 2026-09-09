# Approved production manifest

Все элементы required и привязаны к сценам narrative package.

## Locations

`bg01-sound-archive-night` … `bg10-square-dawn`, а также расширение `bg11-archive-vault`, `bg12-tram-depot-office`, `bg13-radio-transmitter-roof`, `bg14-night-kiosk`, `bg15-resonance-lab`, `bg16-lost-property-hall` — landscape PNG, 16:9, без текста/логотипов; единый painterly neo-noir профиль «Ночелесья». Каждый новый фон используется ровно в содержательной сцене расширенного Ink.

## Characters

Whole-image transparent PNG under `Assets/Characters/<runtime-id>/view/whole/<outfit>/<variant>.png`. Runtime ID Лады — `maincharacter`, остальных — lowercase selector; нейтральный default — `main.png`. Scene-derived variants перечислены в narrative package: 20 PNG, из них 16 адресов используются текущими ветками, остальные четыре — обязательные defaults. Identity masters, contact sheets и alpha proofs обязательны до character handoff.

## Presentation

`Config/cover.png`; story-local `Assets/Presentation/bubble/screen-variant.prefab`; transparent surfaces `dialogue-panel.png`, `choice-card.png`; сдержанный статический медный акцент без встроенного текста и мигания. Анимированный импульс не реализован; см. [BUBBLE_PRESENTATION.md](BUBBLE_PRESENTATION.md).

## Episode catalog covers

Шесть самостоятельных обложек, required по запросу автора:
`Config/EpisodeCovers/s01e01.png` … `s01e06.png`. Все — 1024×1536,
непрозрачный RGB PNG; соответствуют стабильным ID эпизодов, но `_catalogCover`
пока не назначен. Это внешние изображения каталога, а не Unity Sprites или
новые фоны Ink. Общая обложка истории сохранена как fallback.

Предметы, назначения, точные промпты, hashes и самостоятельная проверка полного
набора: [EPISODE_COVERS.md](EPISODE_COVERS.md). Арт подготовлен; назначение
через Inspector и проверка экспортированного preview/каталога остаются pending.

## Audio

Loops: `archive-hum`, `radio-bed`, `water-drone`, `resonance-bed`, `dawn-city`. SFX: `reel-click`, `tram-wire`, `distant-bell`, `tuner-sweep`, `relay-chatter`, `metal-collapse`, `roof-wind`, `cable-strike`, `footsteps`, `thirteenth-bell`, `breaker`. Оригинальный синтез/foley, WAV, без сторонних семплов. Каждый cue используется Ink.

Status: the final-choice correction did not change character/location/audio art; the later author-requested six-cover package extends this manifest with its own review evidence. Character iteration 2 and unchanged asset reviews remain applicable. Current full narrative/Ink originality passed on 2026-09-08 after the explicitly authorized additional review; see `ORIGINALITY_EVIDENCE.md`. Sixteen location files and all used character/audio addresses pass the source check. Unity import, cover assignment/export and Player composition remain final-slot gates.
