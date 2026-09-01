# Agent: tzm-wardrobe-chunk-inclusion

- Status: ready-for-integration
- Task: исправить загрузку TZM wardrobe prefab variant, который сейчас отсутствует в собранном story release и заменяется runtime fallback.
- Scope: `Projects/novels-tzm/Assets/tzm.asset`; временный Unity Editor builder в `Projects/novels-tzm/Assets/Editor/**`; focused TZM validation/build; собственные coordination records и handoff.
- Contract: Ink, общий SDK и поведение гардероба не меняются; существующий GUID `4bb1c2193bcf4b238a510594d07de05f` добавляется в bootstrap content chunk TZM штатной Unity serialization; только Unity Personal-compatible API.
- Base commit: `8f89f27b0f01` плюс сохранённый общий dirty tree.
- Requested UTC: `2026-08-31T16:52:24Z`.
- Acquired UTC: `2026-08-31T16:53:00Z`.
- Result: GUID wardrobe prefab variant добавлен в bootstrap chunk TZM; свежий editor release содержит runtime-адрес `Assets/RemoteAssets/content/tzm/story/presentation/wardrobe/screen-variant.prefab`, поэтому `EpisodeAssetLoader` больше не должен получать `null` и создавать общий fallback.
- Validation: `novels-content validate tzm` passed; `novels-content build tzm editor` passed; build log содержит `Assets/Presentation/wardrobe/screen-variant.prefab`; composed `release.json` содержит точный runtime-адрес; scoped `git diff --check` passed; attach compile открытого Novels Editor passed без compiler errors.
- Manual gate: перезапустить Play Mode/эпизод, поскольку уже созданный fallback-экземпляр не заменяется hot-swap после content rebuild.
