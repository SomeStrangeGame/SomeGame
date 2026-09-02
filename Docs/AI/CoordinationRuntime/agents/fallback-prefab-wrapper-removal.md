# Agent: `fallback-prefab-wrapper-removal`

- Status: ready-for-integration
- Task: убрать пустой game-level слой fallback-prefab для loading, location, character и notification.
- Scope: `Novels/Assets/Novels/Novels.unity`; four `Novels/Assets/Novels/Fallbacks/EpisodeUI/{loading,location,character,notification}/screen-variant.prefab*`; empty folder metadata for loading/location/character; focused Novels serialization/compile validation; own coordination records and shared handoff.
- Contract: `EntryPoint` ссылается непосредственно на полноценные shared base prefab из `Packages/NovelsContentSdk/BaseUI/Base`; story-specific prefab variants продолжают наследовать те же shared bases; notification fallback font asset сохраняется, потому что shared notification prefab ссылается на его GUID.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: `2026-09-01T15:53:00Z`
- Risk: high-impact scene serialization; менять только четыре prefab references, сохранить root GameObject file IDs и проверить отсутствие ссылок на удаляемые GUID.
- Validation plan: static GUID/fileID audit, scoped diff-check, attached Novels compile and manual runtime gate if needed.
- Result: `Novels.unity` теперь ссылается непосредственно на shared loading/location/character/notification prefab; четыре пустых wrapper prefab и meta удалены, пустые loading/location/character folders удалены; notification folder/font сохранены как реальная зависимость shared prefab.
- Serialization: все четыре shared prefab содержат root GameObject fileID `3712146450823894313`; удалённые wrapper GUID больше нигде не встречаются; bubble остаётся direct shared reference.
- Validation: focused GUID/fileID and dependency audit passed; scoped `git diff --check` passed; attached Novels Editor compile passed без compiler errors.
- Pending: optional bounded visual replay fallback UI; story-specific variants не менялись.
- Coordination: `HANDOFF.md` compacted to 113 lines; follow-up validation passed.
