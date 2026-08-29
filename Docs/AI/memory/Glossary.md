# Glossary

| Термин | Значение |
| --- | --- |
| Game | Unity-проект `Novels`, выпускающий Player |
| Content SDK | `Packages/NovelsContentSdk`: общие runtime/editor контракты |
| Atomic project | отдельный Unity-проект Catalog или одной истории |
| Catalog | registry и визуальный bundle выбора историй |
| Story | отдельный контентный проект с `Config/card.json` |
| Authoring assets | исходные Ink, изображения, audio/video и конфиги проекта |
| Published content | release JSON, bundles и content-addressed files для runtime |
| Compose | формирование `Novels/Build/LocalContent` из собранных releases |
| Content source | реализация `IContentSource`: filesystem или HTTP(S) |
| Changed-path gate | минимальная проверка, выбранная по изменённым путям |
| Editor check | один bounded MCP workflow для status/Console/compile/tests |
| FIFO/write-lock | общая очередь и единственное право на запись/Unity heavy process |
| Handoff | короткий актуальный снимок незавершённой межчатовой работы |
| Memory bank | компактная устойчивая память; не runtime-журнал и не архив |
| Whole variant | один цельный sprite персонажа для pose/outfit/emotion |
