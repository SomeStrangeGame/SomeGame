# Agent: gpl-lea-chin-seam

- Status: ready-for-integration
- Task: устранить видимый шов между общим слоем головы Леи и слоями одежды, перенеся границу точно под подбородок.
- Scope: `Projects/novels-gpl/Art/Lea/Layers/**`; `Projects/novels-gpl/Assets/Characters/maincharacter/view/main.png`; `Projects/novels-gpl/Assets/Characters/maincharacter/view/emotions/*.png`; четыре `Projects/novels-gpl/Assets/Characters/maincharacter/clothes/*/1.png`; собственные coordination records; shared handoff.
- Base commit: `4bfd64af41d3`.
- Preserve: исходные цельные варианты, эмоции, позы, регистрация 1024×1536, лицо и волосы не перерисовывать.
- Acceptance: вся шея и ворот принадлежат одежде; общий слой заканчивается под подбородком; reverse composites не имеют шва на тёмной и светлой подложках; GPL validate/build pass.

## Result

- Граница общего head-layer перенесена на `Y=228`; слой головы заканчивается сразу под подбородком с трёхпиксельным перекрытием поверх одежды.
- Все четыре clothes-layer содержат полную шею и ворот начиная с той же границы; пять facial emotion patches также обрезаны по ней.
- `Art/Lea/Layers/proofs/chin-seam-dark-light.png` проверяет четыре одежды на тёмной и светлой подложках без прежнего шва.
- `Tools/novels-tools/novels-content validate gpl` passed; `Tools/novels-tools/novels-content build gpl editor` passed.
- Pending: visual gate в реальной сцене Unity.
