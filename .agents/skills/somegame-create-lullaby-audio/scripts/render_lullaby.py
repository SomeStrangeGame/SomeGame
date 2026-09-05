#!/usr/bin/env python3
"""Render a short lullaby loop from a real pitched one-shot sample."""

import argparse
import math
import struct
import subprocess
import tempfile
import wave
from pathlib import Path


PATTERN = [0, 4, 7, 4, 2, 5, 9, 5, 4, 7, 9, 7, 5, 4, 2, 0, 7, 4, 2, 0]


def convert_note(ffmpeg, source, output, semitones, rate, note_seconds):
    factor = 2 ** (semitones / 12)
    fade_start = max(0.08, note_seconds - 0.20)
    audio_filter = (
        f"atrim=0:{note_seconds},asetpts=PTS-STARTPTS,"
        f"asetrate={rate}*{factor:.9f},aresample={rate},"
        "highpass=f=150,lowpass=f=3200,"
        f"afade=t=in:st=0:d=0.025,afade=t=out:st={fade_start}:d=0.18"
    )
    subprocess.run(
        [ffmpeg, "-v", "error", "-y", "-i", str(source), "-af", audio_filter,
         "-ac", "1", "-c:a", "pcm_s16le", str(output)],
        check=True,
    )


def read_mono(path):
    with wave.open(str(path), "rb") as wav:
        if wav.getnchannels() != 1 or wav.getsampwidth() != 2:
            raise ValueError(f"Expected mono 16-bit WAV after conversion: {path}")
        frames = wav.readframes(wav.getnframes())
    values = struct.unpack("<" + "h" * (len(frames) // 2), frames)
    return [value / 32768.0 for value in values]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--sample", required=True, type=Path,
                        help="Softly played one-shot at the pattern root pitch")
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--duration", type=float, default=15.0)
    parser.add_argument("--rate", type=int, default=44100)
    parser.add_argument("--note-seconds", type=float, default=0.70)
    parser.add_argument("--ffmpeg", default="ffmpeg")
    args = parser.parse_args()

    if args.duration <= 0 or args.rate <= 0 or args.note_seconds <= 0:
        parser.error("duration, rate, and note-seconds must be positive")
    if not args.sample.is_file():
        parser.error(f"sample does not exist: {args.sample}")

    frame_count = round(args.duration * args.rate)
    left = [0.0] * frame_count
    right = [0.0] * frame_count
    spacing = args.duration / len(PATTERN)

    with tempfile.TemporaryDirectory(prefix="lullaby-render-") as tmp:
        note_cache = {}
        for semitone in sorted(set(PATTERN)):
            note_path = Path(tmp) / f"note-{semitone}.wav"
            convert_note(args.ffmpeg, args.sample, note_path, semitone,
                         args.rate, args.note_seconds)
            note_cache[semitone] = read_mono(note_path)

        for index, semitone in enumerate(PATTERN):
            start = round(index * spacing * args.rate)
            pan = -0.14 if index % 2 == 0 else 0.14
            left_gain = math.sqrt((1 - pan) * 0.5)
            right_gain = math.sqrt((1 + pan) * 0.5)
            for offset, sample in enumerate(note_cache[semitone]):
                destination = start + offset
                if destination >= frame_count:
                    break
                left[destination] += sample * left_gain
                right[destination] += sample * right_gain

    peak = max(1e-9, max(map(abs, left)), max(map(abs, right)))
    gain = min(1.0, 0.18 / peak)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(args.output), "wb") as wav:
        wav.setnchannels(2)
        wav.setsampwidth(2)
        wav.setframerate(args.rate)
        block = bytearray()
        for l_value, r_value in zip(left, right):
            l_int = int(max(-1.0, min(1.0, l_value * gain)) * 32767)
            r_int = int(max(-1.0, min(1.0, r_value * gain)) * 32767)
            block += struct.pack("<hh", l_int, r_int)
            if len(block) >= 262144:
                wav.writeframesraw(block)
                block.clear()
        if block:
            wav.writeframesraw(block)

    print(args.output.resolve())


if __name__ == "__main__":
    main()
