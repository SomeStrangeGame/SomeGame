---
name: somegame-create-card-video
description: Create and iterate short looped videos with scene-matched sound from SomeGame story cards or cover images. Use for animated catalog cards, motion covers, or five-to-ten-second audiovisual loops; do not use for full trailers or dialogue-led video.
---

# SomeGame story-card video

Turn an existing story card into a polished short loop whose motion and sound come from visible scene elements. Deliver a playable video artifact; do not modify or import the source card unless the user separately asks for that.

## Locate the source

When working inside SomeGame, load `somegame-workflow` before inspecting project files. Search `Projects/novels-*/Config/card.json` and `cover.png` in the current checkout first. If the story is absent, inspect `git worktree list --porcelain`, select the worktree whose branch or path matches the story, and confirm its `card.json` title before using its cover.

Treat worktree contents as potentially in progress. Read the image but preserve every existing change. Render drafts outside the repository, preferably in the task artifact directory or a fresh temporary directory.

## Design from the image

Inspect the cover at full resolution and identify:

- foreground, middle-ground, and background planes;
- elements that can plausibly move, such as snow, rain, smoke, fabric, hair, foliage, lamps, windows, water, hanging ornaments, or vehicles;
- objects that can plausibly make discrete sounds;
- the visual focus that camera movement should support.

Choose two or three complementary motions instead of applying one global zoom. Useful combinations include depth-separated particles, restrained camera travel, local light changes, parallax, object sway, and a foreground pass. Keep faces and authored composition stable unless the user requests a stronger transformation.

For a loop, drive motion with periodic curves or make the first and last rendered states match. Check the seam in motion, not only as still frames. Avoid random per-frame effects that jump at the boundary; use a tiled particle field or another repeatable cycle.

## Build scene-matched sound

Use a quiet environmental bed only to establish space. The expressive layer must consist of recognizable events tied to visible objects—for example:

- hanging paper or metal decorations: sparse sway, paper flex, or light chimes;
- footsteps or people on snow: isolated snow compression and crunch;
- lit windows or interiors: distant room tone, door movement, or a muted transient when visually justified;
- trees and loose branches: occasional branch movement rather than continuous broadband rustle;
- snowfall: usually near-silent; do not represent it with steady white noise.

Prefer recorded or convincingly synthesized foley-like transients over stationary tones. Vary event timing, pitch, stereo position, and decay. Do not use a continuous sine bed that reads as hum. Do not add paper rustle merely because paper is visible: use it only when the animation clearly moves the paper and remove it completely when the user rejects it.

Keep audio layers separable until approval so a rejected sound can be removed without degrading the video or rebuilding accepted layers. When revising, preserve the approved visual stream and remux a replacement mix if only audio changes.

## Render and iterate

Honor the requested duration, aspect ratio, and container. If unspecified, preserve the source aspect ratio and use H.264 video with AAC stereo audio in MP4 at 30 fps. A five-second card loop should contain exactly 150 frames at 30 fps.

Use the best available video tool. FFmpeg is an acceptable local fallback for camera motion, repeatable particles, compositing, synthesized ambience, foley placement, and remuxing. Never claim that a static zoom is a fully animated scene; state the actual treatment when delivering it.

Translate perceptual feedback into the responsible layer:

- “not dynamic” -> increase depth-separated motion or camera travel, not global brightness alone;
- “sound is noise” -> lower the environmental bed and add sparse object-linked events;
- “monotonous” -> vary discrete timing, timbre, stereo placement, and decay;
- “remove the rustle” -> remove the rustle stem entirely rather than equalizing the final mix;
- “loop jumps” -> align periodic visual state and audio tail at the seam.

## Validate and deliver

Before delivery:

1. Inspect a representative frame and compare it with the source for color and composition.
2. Confirm duration, frame rate, dimensions, video codec, audio codec, sample rate, and channel count with a media probe.
3. Verify that the expected number of frames was rendered and that audio is non-silent without clipping.
4. Review the seam and confirm rejected sound layers are absent.

Return the latest version as a playable absolute-path link. Name revisions descriptively, such as `story-card-loop-5s-v2-no-rustle.mp4`, and keep the previously approved version available during iteration.
