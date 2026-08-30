# Agent: gpl-lea-layered-rework

- Status: ready-for-integration
- Task: переделать Лею на независимые технические слои одежды и эмоций с доказанной обратной сборкой.
- Scope: `Projects/novels-gpl/Art/Lea/**`; `Projects/novels-gpl/Assets/Characters/maincharacter/**`; `Projects/novels-gpl/Assets/gpl.asset` if required; own coordination records; shared handoff.
- Base commit: `4bfd64af41d3`.
- Rollback: существующий `view/whole/**` не удалять до успешного layered proof и Unity validation.
- Acceptance: общий 1024×1536 холст, настоящая alpha, base/clothes/emotion не содержат дублирующий полный силуэт, обратная сборка без швов, одежда и эмоция переключаются независимо.

## Result

- `view/main.png` содержит общую голову, волосы и нейтральное лицо; `view/emotions/**` — только зарегистрированные лицевые patches.
- Каждый `clothes/<name>/1.png` целиком содержит ворот/видимую шею, торс, руки, кисти, ноги и обувь; отдельных общих hands/boots нет.
- В Unity импортированы четыре независимых комплекта и пять эмоций; цельными оставлены только три сюжетные позы `flashlight`, `tablet`, `reach_lever`.
- Исходные цельные варианты сохранены в `Art/Lea/WholeRollback`; proofs находятся в `Art/Lea/Layers/proofs`.
- Validation: `Tools/novels-tools/novels-content validate gpl` passed; `Tools/novels-tools/novels-content build gpl editor` passed; release содержит layered base/clothes/emotions и только три ожидаемые whole poses.
- Pending: bounded visual gate непосредственно в сцене Unity; контентная адресация и сборка завершены.
