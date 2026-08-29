# SomeGame Ink v1 patch

This package is pinned from upstream commit
`770b372042728fc9df9d26ae19287cf6145d47a5` (`1.2.2`).

`1.2.2-somegame.1` fixes the legacy asynchronous compiler lifecycle when Unity
reloads assemblies:

- a persisted `Compiling` item is re-queued after domain reload;
- its abandoned worker result, logs and timing are reset;
- a fresh Unity Progress operation is created;
- new workers are deferred while `EditorApplication.isCompiling` is true;
- removal of a missing Progress operation is guarded.

Do not edit `Library/PackageCache`. Apply future upstream changes to this pinned
package and repeat TZM/ZDM restart validation before changing the dependency.
