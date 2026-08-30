# Agent: gpl-vera-pose-hair

- Status: ready-for-integration
- Final: утверждённые compact-hair кандидаты promoted в canonical `station_urgent.png`, `station_hides_hands.png`, `station_pain.png`; вместе с neutral все четыре 1024x1536 RGBA. Сохранены full/face verification sheets; Unity import pending.
- Pain result: создан `station_pain_compact_hair.png`, 1024x1536 RGBA; болезненная поза и захват предплечья сохранены, коса заменена компактным пучком. Ожидается visual approval и затем общий лист/замена canonical файлов.
- Correction result: создан `station_hides_hands_compact_hair_v2.png` — руки естественно уведены и полностью скрыты за поясницей, локти близко к корпусу; 1024x1536 RGBA. Canonical ожидает visual approval.
- Correction: `hides_hands` отклонён — кисти неестественно торчат по бокам; перегенерировать цельную позу с руками, действительно соединёнными и скрытыми за поясницей.
- Task: перенести утверждённую компактную причёску Веры в три цельные сюжетные позы.
- Scope: `Projects/novels-gpl/Art/Vera/Variants/station_urgent.png`, `station_hides_hands.png`, `station_pain.png`; Vera proof images; own coordination records; shared handoff.
- Base commit: `4bfd64af41d3`.
- Acceptance: каждый 1024x1536 RGBA-вариант сохраняет исходные лицо, эмоцию, тело, одежду и позу; меняется только коса на компактный пучок, не пересекающий шею/воротник.
- Result: первый urgent-кандидат создан и сохранён рядом с исходником; причёска визуально корректна, но генератор вернул RGB с нарисованной шахматкой, поэтому кандидат не production и canonical не заменён. После visual approval повторить на solid-green для alpha, затем сделать две оставшиеся позы.
- Progress: `station_urgent_compact_hair.png` и `station_hides_hands_compact_hair.png` подготовлены как 1024x1536 RGBA через solid-green fallback; исходные canonical пока не заменены. Осталась `station_pain` и общий visual sheet.
