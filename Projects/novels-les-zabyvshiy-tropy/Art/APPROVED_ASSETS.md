# Approved production manifest

Стиль «Ночелесья»: живописная 2D-гуашь с мягким углём, хвойные зелёные и индиго, редкие янтарные ориентиры, без текста и чужой IP. Персонажи — цельные прозрачные PNG. Фактические фоны имеют 627×627, что отличается от первоначального требования 16:9. Runtime масштабирует их по высоте доступной области (`LocationLayout.SetVisualSize`), а не гарантирует 16:9 crop. Соответствие композиции экрану остаётся незакрытым visual gate; прежнее переописание формата как уже одобренного было ошибочным.

## Required character variants

- Ника/`fieldcoat`: main, alert, tender, determined.
- Ася/`forester`: main, stern, guilty, relieved.
- Филя/`windbreaker`: main, grinning, afraid, focused.
- Лада/`ringcoat`: main, curious, sad, luminous.
- Яр/`trailjacket`: main, fading, hopeful, exhausted.

Адрес: `story/character/characters/<имя>/view/whole/<outfit>/<variant>.png`.

Исключение главной героини: отображаемое имя остаётся «Ника», но её runtime ID и физический каталог — `maincharacter`, то есть `Assets/Characters/maincharacter/view/whole/fieldcoat/`. Остальные имена каталогов остаются в нижнем регистре.

## Required scene art

`bg01-ranger-station-dusk`, `bg02-erased-cutline`, `bg03-blackwater-creek`, `bg04-inverted-bridge`, `bg05-doorless-cabin`, `bg06-map-room`, `bg07-voice-ring`, `bg08-lantern-bog`, `bg09-unsaid-names-glade`, `bg10-root-arch`, `bg11-first-trail-dawn`, `bg12-ranger-station-morning`, `bg13-fire-lookout-night`, `bg14-stone-orchard`, `bg15-whisper-archive`, `bg16-dawn-ravine`. `bg04`, `bg08`, `bg10`, `bg13`–`bg16` — отдельные сюжетные reveal-композиции, а не варианты заполнения.

Choice art: `anchor-name`, `anchor-promise`, `anchor-trace`, `follow-lada`, `trust-compass`, `keep-map`, `burn-map`, `confess-fear`, `accept-echo`, `release-knot`, `bind-road`, `share-memory`.

Bubble: `Presentation/bubble/screen-variant.prefab`, sprites `dialogue-panel.png`, `choice-card.png`, доступный текстовый fallback. Prefab сохраняет проверенный runtime contract и GUID, а его поверхности заменены на оригинальные story-local sprites «Ночелесья».

Audio: loops `nightwood-bed.wav`, `dawn-thread.wav`; SFX `bell-single`, `path-whisper`, `blackwater`, `paper-breath`, `root-pulse`, `first-step`.

## Episode catalog covers — added 2026-09-08

Автор запросил отдельную обложку каждого из шести эпизодов. Созданы шесть разных RGB PNG 1024×1536: `Config/EpisodeCovers/s01e01.png` … `s01e06.png`; назначены через `_catalogCover` в definition. Это отдельные иллюстрации для карточек, не дополнительные сюжетные фоны и не копии общей обложки. Манифест, полный prompt set, source-art approval, проверка оригинальности и отложенные gates: `EPISODE_COVERS.md`, `EPISODE_COVER_PROMPTS.md`. Старые ассеты не заменены. Экспорт требует интеграции актуального SDK из main.

## Production/originality — historical scene-art package

Графика создаётся встроенным image generator по оригинальным сценовым спецификациям; варианты выводятся из identity masters. Аудио синтезируется из осцилляторов и шумов без сторонних семплов. Reverse-image/descriptive поиск недоступен; сравнение ограничено общежанровыми образами. Итерация 2 добавила четыре сцены и два выбора; проверенный отличительный набор теперь включает белые незаполненные просеки, мост с отражением сверху, вышку-схему внимания, сад безымянных свидетельств, архив дыхания в банках, плетёные мосты оврага и корневую арку обещаний. Риск low, confidence medium. Gate расширенного манифеста: `passed`; runtime visual proof отложен.
