# Agent: tzm-video-30fps

- Status: completed
- Task: transcode only 60 FPS TZM videos to 30 FPS
- Branch: `experiment/story-preview-streaming`
- Worktree: `/Users/iantonishin/Documents/Codex/SomeGame-story-preview-experiment`
- Scope: `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/*.mp4`, TZM generated release outputs, own coordination records
- Contract: preserve source dimensions/aspect, H.264 CRF 18, remove audio, leave existing 30 FPS files byte-identical
- Expected validation: ffprobe all 51 files; 30 FPS, no audio, unchanged dimensions; rebuild TZM streaming release
- Result: 37 source videos converted; 14 existing 30 FPS files unchanged; commit `a5dbfd2b`
- Result: video payload 287,268,760 -> 263,354,915 bytes; saved 23,913,845 bytes (8.3%)
- Result: Editor streaming release `3ac58d77fdbc8eff66c4f6dabc84d4f2e7c559b9e81b88dbad5e4d610b991ab5` built and composed
- Validation: all 51 release videos are 30 FPS, one video stream, zero audio streams; source dimensions preserved
