# Approved production manifest

Стиль «Ночелесья»: живописная 2D-гуашь с мягким углём, хвойные зелёные и индиго, редкие янтарные ориентиры, без текста и чужой IP. Фоны 16:9 с безопасным центром; персонажи — цельные прозрачные PNG.

## Required character variants

- Ника/`fieldcoat`: main, alert, tender, determined.
- Ася/`forester`: main, stern, guilty, relieved.
- Филя/`windbreaker`: main, grinning, afraid, focused.
- Лада/`ringcoat`: main, curious, sad, luminous.
- Яр/`trailjacket`: main, fading, hopeful.

Адрес: `story/character/characters/<имя>/view/whole/<outfit>/<variant>.png`.

## Required scene art

`bg01-ranger-station-dusk`, `bg02-erased-cutline`, `bg03-blackwater-creek`, `bg04-inverted-bridge`, `bg05-doorless-cabin`, `bg06-map-room`, `bg07-voice-ring`, `bg08-lantern-bog`, `bg09-unsaid-names-glade`, `bg10-root-arch`, `bg11-first-trail-dawn`, `bg12-ranger-station-morning`, `bg13-fire-lookout-night`, `bg14-stone-orchard`, `bg15-whisper-archive`, `bg16-dawn-ravine`. `bg04`, `bg08`, `bg10`, `bg13`–`bg16` — отдельные сюжетные reveal-композиции, а не варианты заполнения.

Choice art: `anchor-name`, `anchor-promise`, `anchor-trace`, `follow-lada`, `trust-compass`, `keep-map`, `burn-map`, `confess-fear`, `accept-echo`, `release-knot`, `bind-road`, `share-memory`.

Bubble: `Presentation/bubble/screen-variant.prefab`, sprites `dialogue-panel.png`, `choice-card.png`, доступный текстовый fallback. Prefab сохраняет проверенный runtime contract и GUID, а его поверхности заменены на оригинальные story-local sprites «Ночелесья».

Audio: loops `nightwood-bed.wav`, `dawn-thread.wav`; SFX `bell-single`, `path-whisper`, `blackwater`, `paper-breath`, `root-pulse`, `first-step`.

## Production/originality

Графика создаётся встроенным image generator по оригинальным сценовым спецификациям; варианты выводятся из identity masters. Аудио синтезируется из осцилляторов и шумов без сторонних семплов. Reverse-image/descriptive поиск недоступен; сравнение ограничено общежанровыми образами. Итерация 2 добавила четыре сцены и два выбора; проверенный отличительный набор теперь включает белые незаполненные просеки, мост с отражением сверху, вышку-схему внимания, сад безымянных свидетельств, архив дыхания в банках, плетёные мосты оврага и корневую арку обещаний. Риск low, confidence medium. Gate расширенного манифеста: `passed`; runtime visual proof отложен.
