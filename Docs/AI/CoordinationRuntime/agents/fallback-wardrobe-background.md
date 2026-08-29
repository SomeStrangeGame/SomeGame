# Agent: fallback-wardrobe-background

- Статус: yielded
- Задача: добавить собственный фон и обновить системный fallback гардероба по утверждённому мокапу
- Область:
  - `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/**`
  - `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/**`
  - минимальные Game/runtime integration points для передачи фона
  - системные fallback-ассеты гардероба и их `.meta`
  - собственные coordination-записи
- Ограничения: не менять Ink; не менять Android texture profile; не добавлять переключение персонажей
- Ожидаемый результат: отдельный фон гардероба, категории/варианты в компоновке мокапа, live preview и подтверждение сохраняют текущее поведение
- Результат: runtime layout реализован; C# fallback build и Unity batch compile успешны; остаётся ручной visual check в Play Mode
