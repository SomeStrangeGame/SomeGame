#!/usr/bin/env python3
"""Convert imagegen checker backgrounds to real alpha and exact Unity sizes."""

from collections import deque
from pathlib import Path
import sys

from PIL import Image


def is_checker(pixel: tuple[int, int, int, int]) -> bool:
    red, green, blue, _ = pixel
    return max(red, green, blue) - min(red, green, blue) <= 12 and red >= 145


def clear_connected_background(image: Image.Image, include_center: bool) -> Image.Image:
    result = image.convert("RGBA")
    width, height = result.size
    seeds = [(x, 0) for x in range(width)] + [(x, height - 1) for x in range(width)]
    seeds += [(0, y) for y in range(height)] + [(width - 1, y) for y in range(height)]
    if include_center:
        seeds.append((width // 2, height // 2))

    queued = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()
    for x, y in seeds:
        index = y * width + x
        if not queued[index] and is_checker(result.getpixel((x, y))):
            queued[index] = 1
            queue.append((x, y))

    while queue:
        x, y = queue.popleft()
        red, green, blue, _ = result.getpixel((x, y))
        result.putpixel((x, y), (red, green, blue, 0))
        for next_x, next_y in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            if not (0 <= next_x < width and 0 <= next_y < height):
                continue
            index = next_y * width + next_x
            if queued[index] or not is_checker(result.getpixel((next_x, next_y))):
                continue
            queued[index] = 1
            queue.append((next_x, next_y))
    return result


def main() -> None:
    if len(sys.argv) != 5:
        raise SystemExit("usage: script dialogue-raw choice-raw dialogue-out choice-out")
    dialogue_raw, choice_raw, dialogue_out, choice_out = map(Path, sys.argv[1:])
    dialogue = clear_connected_background(Image.open(dialogue_raw), include_center=False)
    choice = clear_connected_background(Image.open(choice_raw), include_center=True)
    dialogue.resize((1200, 800), Image.Resampling.LANCZOS).save(dialogue_out)
    choice.resize((512, 768), Image.Resampling.LANCZOS).save(choice_out)


if __name__ == "__main__":
    main()
