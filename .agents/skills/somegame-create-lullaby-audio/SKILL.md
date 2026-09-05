---
name: somegame-create-lullaby-audio
description: Create or revise short, calm, seamlessly looped lullaby audio for SomeGame when the user asks for soothing background music, bedtime melodies, or gentle musical ambience.
---

# Create lullaby audio

Produce an original, quiet musical loop that remains comfortable under repeated
listening. Prefer a real, softly played one-shot sample over sustained oscillator
synthesis. Use `scripts/render_lullaby.py` when a suitable one-shot is available.

## Establish the direction

Confirm or reasonably infer the duration, lead instrument, mood, and whether a
natural ambience is wanted. When the user supplies a reference, extract broad
traits such as density, register, articulation, and pacing; do not reproduce its
melody.

For a first draft, favor:

- 12–20 seconds for an in-game loop;
- one clear, singable phrase with a return to its tonal home;
- softly struck marimba, felt piano, kalimba, harp, or nylon-string one-shots;
- a comfortable middle register and moderate stereo width;
- sparse accompaniment, or none.

## Avoid listener fatigue

Do not fill silence by sustaining pure sine waves, stacked drones, or unchanging
bass notes. These easily become a hum on headphones and small speakers.

Keep attacks rounded but recognizable. Avoid hard plucks, bright metallic
partials, persistent hiss, aggressive limiting, binaural beats, and large stereo
phase differences. Overlap only the natural tails of adjacent notes; do not turn
the phrase into a continuous resonance.

If the melody feels disconnected, first shorten the gap between note onsets or
add a quiet answering voice. Do not immediately add a pad or noise bed.

## Add ambience only when it reads as an event

Natural ambience is optional. A constant recording of waves, wind, or rain can
sound like interference when mixed too evenly. Shape it into recognizable,
occasional events—such as two or three wave swells per loop—with quiet intervals
between them. Keep ambience clearly below the melody and remove rumble and hiss.

Treat “fireflies” as sparse, rounded glints in different stereo positions, not
as insect noise or a continuous high-frequency layer.

Read [references/audio-quality.md](references/audio-quality.md) when diagnosing
hum, harshness, audible pauses, or loop seams.

## Render and verify

Use PCM WAV at 44.1 or 48 kHz as the authoring master. Preserve source and
license attribution for every external sample. Do not claim an output is
royalty-free unless each source and its permitted use were verified.

After rendering:

1. Check exact duration, channels, sample rate, peak, and RMS level.
2. Compare the last-to-first sample transition and listen across at least three
   consecutive repetitions.
3. Audition on headphones and a small speaker at low volume.
4. Check separately for tonal hum, sharp attacks, perceived noise, dead air,
   and whether the melody remains recognizable.
5. Keep prior drafts unless the user explicitly asks to replace them.

If an artifact is created outside the Unity project, return a playable absolute
file link. Import into project Assets only when the user asks, then follow the
repository coordination and content validation workflow.
