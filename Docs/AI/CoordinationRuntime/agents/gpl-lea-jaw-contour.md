# Agent: gpl-lea-jaw-contour

- Status: ready-for-integration
- Task: заменить горизонтальный срез головы Леи на точную маску по контуру нижней челюсти и подбородка.
- Scope: `Projects/novels-gpl/Art/Lea/Layers/**`; соответствующие layered PNG в `Projects/novels-gpl/Assets/Characters/maincharacter/**`; own coordination records; shared handoff.
- Base commit: `4bfd64af41d3`.
- Acceptance: общий слой не содержит пикселей шеи; шея и ворот полностью в одежде; jaw-contour reverse proof без швов на dark/light; GPL validate/build pass.
- Checkpoint: пользователь остановил работу до изменения арта. Текущая версия остаётся с горизонтальной границей `Y=228`; при продолжении нужно заново получить FIFO/write-lock и заменить её маской по контуру нижней челюсти.
- Resumed UTC: `2026-08-30T13:40:16Z`; пользователь попросил продолжить.
- Result: горизонтальная граница заменена piecewise jaw-contour маской с 16 px мягким переходом; все четыре clothes-layer и пять emotion patches обновлены; `jaw-contour-dark-light.png` проверен; GPL validate и editor build passed.
