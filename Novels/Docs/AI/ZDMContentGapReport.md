# ZDM content gap report

This report is based on a complete static scan of all 11 authored ZDM Ink episode
files and the character content available under `Визуал ЖДМ/Персонажи`.

## Automatically integrated

- All character folders present in the supplied source directory have been imported.
- `Рамос` has been added to shared ZDM character content.
- `Стражники` has its own strict Ink-to-path identity and currently reuses the
  supplied `Стражник` artwork.
- `переодеть` is treated as a temporary dialogue control rather than an asset name.
- Known display identities can occur in any argument position without becoming
  sprite dependencies.
- Case and Unicode normalization is shared by validation and runtime bundle lookup.

## Character artwork not supplied

No matching source folder exists for these Ink speakers:

| Character | First affected content |
| --- | --- |
| Торговец | `s01e01`, also `s01e09` |
| Врачевательница | `s01e09` |
| Жрец Анубиса | `s01e09` |
| Второй жрец Анубиса | `s01e09` |
| Меритра | `s01e10` |
| Сенмут | `s01e10` |
| Служанка | `s01e10` |
| Паромщик | `s02e01` |
| Акен | `s02e01` |
| Анубис | `s02e01` |
| Хранитель Врат | `s02e01` |
| Вестник | `s02e01` |
| Наблюдатель | `s02e01` |
| Душа | `s02e01` |

These are content gaps rather than address, casing, import, or validator defects.

## Explicit visual decisions still required

- `Фараон (глаза)`
- `Нур (мокрая)`
- `Нур (накидка)`
- `Нур (решительность)`
- missing non-synonymous expressions for Куибила, Джосер, the guards, and any
  characters whose artwork has not yet been supplied

Do not silently map these entries to unrelated emotions. Add authored artwork or
approve an explicit visual fallback before publication.
