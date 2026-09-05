---
name: somegame-create-horror-ambient
description: Create or revise restrained horror ambience for SomeGame, including oppressive tonal beds, environmental creaks, and barely intelligible processed voices. Use when the user asks for dark, tense, unsettling, or horror background audio; do not use for calm lullabies, songs, or dialogue-first voice production.
---

# SomeGame horror ambient

Create an original, usable audio artifact and iterate from what the user hears. Preserve the latest approved mix while changing only the requested layer.

## Start

When working inside SomeGame, load `somegame-workflow` and follow repository coordination before writing project files. Render drafts outside the repository until the user approves them; importing into a Unity project is a separate, explicitly scoped action.

Establish or infer the emotion, intensity, duration, loop requirement, forbidden sounds, desired layers, output format, and destination. If the request is already specific, render without asking the user to restate it.

## Compose in separable layers

Keep intermediate stems so revisions do not degrade approved material:

1. Build the pressure bed from slowly evolving, unresolved low-mid intervals. Avoid a stationary sine or narrow resonant peak that reads as electrical hum.
2. Add environmental details sparsely. A metal creak should use irregular stick-slip catches with very short resonances; a sustained resonator sounds like “singing metal.”
3. Add voices only when requested. For ominous background speech, preserve human timing while suppressing lexical clarity through low level, reduced consonant range, slow delivery, stereo distance, and dark echoes.
4. Apply final dynamics and peak limiting only after the stems are balanced.

For detailed voice and sound-design decisions, read [references/design-guide.md](references/design-guide.md).

## Voice workflow

Use an actual recorded or synthesized phrase as the articulation source. Noise shaped only by an amplitude envelope usually sounds like rustling, not whispering.

For a male whisper when only a female TTS voice is available:

- shift pitch and formants downward together, then restore timing;
- mix a small breath/noise layer under the voiced consonants;
- stretch phonemes with time processing when the user asks for drawn-out words;
- use mild saturation or irregular amplitude modulation for hoarseness;
- reduce intelligibility with level, low-pass filtering, echoes, and optionally a quiet reversed copy—never by deleting the voice entirely.

Verify synthesized source files have non-zero duration before mixing. A valid container with zero audio frames is silent regardless of gain.

## Iterate by perception

Translate feedback into the responsible parameter:

- “not audible” → verify the source first, then raise the stem or carve space in the bed;
- “too clear” → reduce direct-to-echo ratio, attenuate 1–4 kHz consonant detail, or add a reversed diffuse layer;
- “rustling” → restore articulated speech and reduce breath noise;
- “singing metal” → shorten resonances and replace continuous tones with discrete friction catches;
- “too strange/cartoonish” → reduce pitch bends and modulation depth;
- “too long” → shorten the event envelope without increasing attack sharpness.

Change one perceptual dimension per iteration when practical. Name each draft descriptively and return a playable absolute-path WAV link.

## Validate

Before delivery, confirm duration, sample rate, channels, and non-zero audio frames; measure mean and peak level; prevent clipping; inspect the start/end for clicks; and confirm forbidden harsh or humming elements were not introduced. For loops, compare the seam and render a crossfade if necessary. Report sparse event locations when that helps review.

Prefer lossless WAV for iteration. Create compressed delivery formats only after approval.
