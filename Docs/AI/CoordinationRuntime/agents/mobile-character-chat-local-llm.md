# Agent: mobile-character-chat-local-llm

- Status: ready-with-limitations
- Task: заменить demo replies атомарного мобильного чата настоящим локальным LLM inference и временно включить модель в Player build.
- Scope: `Novels/Packages/{manifest.json,packages-lock.json}`, `Novels/Assets/Novels/Novels.asmdef`, `Novels/Assets/Novels/CharacterChat/**`, exact bundled `Novels/Assets/StreamingAssets/{qwen2.5-1.5b-instruct-q4_k_m.gguf,QWEN_MODEL_LICENSE.txt,MODEL_NOTICE.md,LlamaLib-v2.0.4/**}` plus generated metas, focused Unity/Player validation, own coordination records and shared `HANDOFF.md`.
- Contract: inference полностью локальный; чат не читает и не меняет story progress/save; character system prompt содержит каноническую identity; demo backend остаётся только явным fallback при недоступном runtime/model; Android/iOS-compatible Unity Personal dependencies only.
- Base commit: `65cda4506b66`.
- Requested UTC: `2026-08-31T14:33:27Z`.
- Acquired UTC: `2026-08-31T14:33:40Z`.
- Yielded UTC: `2026-08-31T14:35:00Z`; lock was released immediately after the earlier `tzm-wardrobe-visual-check` request became visible. No project files or heavy Unity processes were changed or started.
- Requeued UTC: `2026-08-31T14:35:35Z` as request `20260831T143535Z-mobile-character-chat-local-llm`, behind the visual check.
- Reacquired UTC: `2026-08-31T14:38:00Z` after the preceding visual check released FIFO.
- Model decision: initial official Qwen2.5 0.5B Q4_K_M native smoke succeeded but hallucinated identity facts and then refused the grounded prompt, so it was rejected. Final candidate is official `Qwen/Qwen2.5-1.5B-Instruct-GGUF` Q4_K_M, immutable revision `91cad51170dc346986eccefdc2dd33a9da36ead9`, 1117320736 bytes, SHA-256 `6a1a2eb6d15622bf3c96857206351ba97e1af16c30d7a74ee38970e434e9407e`, Apache-2.0.
- Result: commit `8f89f27b`; real native inference, fresh Unity compile and Android build passed. APK `Novels/Build/Players/character-chat-local-llm.apk` is 2986082570 bytes. Physical-device latency/RAM smoke remains.
