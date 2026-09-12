# Novels Story Runtime

Shared execution used by mobile and WebGL players. `Runtime/Novels.StoryRuntime`
owns episode lifetime, the Ink read/queue loop, and generic queue execution.

The core assembly does not own a catalog, application shell, media, content
delivery, persistence backend, or concrete presentation. Hosts supply these.

`Presentation/Novels.StoryPresentation` contains the existing shared StoryQueue,
Bubble operations, choice handling and replay validation. It depends on Content
SDK controllers, but not on its Audio or Catalog assemblies. Hosts importing
this package must also import Content SDK, NovelInk, UniTask and Disposable.
Native supplies its original audio mapping; web omits audio and wardrobe UI.
Missing wardrobe support rejects wardrobe content instead of choosing for users.
An optional `FlushCheckpoint` callback makes web advance/choice completion wait
for durable storage; absent callback preserves the native synchronous path.

The separate `Save/Novels.Save` assembly owns the shared decision/replay save
logic, not its storage. Its original assembly and asset GUIDs are retained.
The binary v3 envelope and v2 reader are unchanged, including legacy wardrobe
fields; this does not enable a wardrobe UI in the web player.

Native hosts supply byte read/write/delete callbacks. Async hosts additionally
supply `FlushStorageAsync`; `SaveSystem.FlushAsync` waits for both the writer
and the durable storage acknowledgement. Synchronous flush is explicitly
unsupported when this barrier is configured. The web adapter preserves damaged
or incompatible records and reports failure instead of silently restarting.

`Progress/Novels.Progress` owns the existing `NovelProgress` episode unlock and
Ink entry-state logic. NPR1 format, GUID and native Cache constructors remain
compatible; the assembly depends on Content definitions and Cache. A callback
constructor supports browser storage; `strict` rejects incompatible progress
without reset. Web stores the native progress blob and completion markers in
one versioned IndexedDB record, committed after decisions and before advancing.
