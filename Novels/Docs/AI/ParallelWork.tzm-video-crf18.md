# Parallel work: tzm-video-crf18

- Статус: ready-for-integration
- Ветка: grandChange
- Базовый commit: c6c7853b
- Ответственный поток: tzm-video-crf18
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/*.mp4`
- TZM generated releases
- size/status документация и собственные runtime coordination files

## Изменённый контракт

- Видео остаются MP4/H.264/YUV420p с исходными resolution/FPS/duration.
- Все аудиопотоки удалены.
- Видеопотоки перекодированы `libx264 -preset slow -crf 18`.

## Выполнено

- Перекодирован 51 ролик; 49 исходных AAC-дорожек удалены.
- Размер видео: 346 350 427 B → 287 268 760 B.
- Экономия: 59 081 667 B (17,06%).
- Пересобраны Android, Mac/Editor и iOS releases.
- 51 старый неиспользуемый generated payload вынесен из LocalContent.

## Проверено

- Все 51 файла: H.264, YUV420p, исходные resolution и FPS.
- Audio streams: 0.
- Отклонение duration: не более 0,04 секунды.
- `novels-content validate tzm` — успешно.
- `novels-content build tzm android|editor|ios` — успешно.

## Размеры release после изменения

| Платформа | Bundle | Payloads | Итого |
| --- | ---: | ---: | ---: |
| Android | 304 933 451 B | 289 811 340 B | 594 744 791 B |
| Mac | 154 694 278 B | 289 811 340 B | 444 505 618 B |
| iOS | 181 345 930 B | 289 811 340 B | 471 157 270 B |

## Требуется при интеграции

- Ручной просмотр характерных роликов и проверка бесшовного loop.
- Выполнить ручной visual quality gate видео и iOS ASTC 8×8.
- Оригиналы до приёмки находятся в
  `/tmp/novels-tzm-video-originals-20260824T1236Z`.
