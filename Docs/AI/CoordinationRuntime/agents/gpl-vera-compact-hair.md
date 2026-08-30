# Agent: gpl-vera-compact-hair

- Status: awaiting-user-approval
- Task: заменить косу Веры на компактную причёску, не пересекающую шею, ворот или одежду.
- Scope: `Projects/novels-gpl/Art/Vera/**`; own coordination records; shared handoff.
- Base commit: `4bfd64af41d3`.
- Acceptance: новый цельный neutral master сохраняет лицо, тело, позу, одежду, холст и стиль; волосы полностью внутри head silhouette и заканчиваются выше шеи; 1024×1536 RGBA и clean alpha.
- Result: создан `Variants/station_neutral_compact_hair.png` и dark/light/comparison proofs; компактный пучок не пересекает шею, воротник и плечи. Канонический neutral не заменён до visual approval.
- Continuation: причёска одобрена; заменить neutral master и детерминированно пересобрать layered pilot с границей головы под подбородком, затем проверить reverse composition на светлой и тёмной подложках.
- Layer result: compact-hair файл принят как канонический `station_neutral.png`; `LayerPilot/head_base.png` содержит только голову/причёску, `station_clothes.png` — шею и весь образ ниже. Reverse composition pixel-identical (`PSNR inf`) исходному RGBA; proofs обновлены.
- Correction: visual gate выявил крупный фрагмент воротника в head layer; требуется более высокая граница строго под подбородком.
- Correction result: маска заново построена по сужающемуся контуру нижней челюсти; head заканчивается на подбородке без шеи/воротника, clothes содержит весь остаток. Reverse composition снова pixel-identical (`PSNR inf`).
