# novels-tools

Локальный последовательный оркестратор content-проектов. Он намеренно не
запускает несколько Unity Editor одновременно.

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content build-local all
Tools/novels-tools/novels-content publish-local /path/to/server-root
```

Путь к Unity можно переопределить через `NOVELS_UNITY_BIN`, а статический
build entry point — через `NOVELS_BUILD_METHOD`.
