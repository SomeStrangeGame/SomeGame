#!/usr/bin/env python3
"""Apply reviewed per-character masks without changing any RGB pixels."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw


ART_DIR = Path(__file__).resolve().parent
STORY_DIR = ART_DIR.parents[1]
CHARACTERS_DIR = STORY_DIR / "Assets" / "Characters"
MASKS_DIR = ART_DIR / "masks"

CHARACTERS = {
    "maincharacter": CHARACTERS_DIR / "maincharacter/view/whole/fieldcoat",
    "asya": CHARACTERS_DIR / "ася/view/whole/forester",
    "lada": CHARACTERS_DIR / "лада/view/whole/ringcoat",
    "filya": CHARACTERS_DIR / "филя/view/whole/windbreaker",
    "yar": CHARACTERS_DIR / "яр/view/whole/trailjacket",
}


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    report: dict[str, object] = {
        "method": "reviewed per-character alpha mask; original RGB preserved byte-for-byte after decode",
        "characters": {},
    }
    proof = Image.new("RGB", (1024, 768 * len(CHARACTERS)), (0, 0, 0))
    draw = ImageDraw.Draw(proof)
    variants_proof = Image.new("RGB", (1024, 384 * len(CHARACTERS)), (0, 0, 0))
    variants_draw = ImageDraw.Draw(variants_proof)

    for row, (selector, directory) in enumerate(CHARACTERS.items()):
        entries = []
        image_paths = sorted(directory.glob("*.png"))
        for column, image_path in enumerate(image_paths):
            mask_path = MASKS_DIR / f"{selector}__{image_path.stem}.png"
            mask = Image.open(mask_path).convert("L")
            if mask.size != (512, 768):
                raise ValueError(f"Unexpected mask dimensions: {mask_path} {mask.size}")
            before = Image.open(image_path).convert("RGBA")
            before_pixels = np.asarray(before).copy()
            if before.size != mask.size:
                raise ValueError(f"Unexpected sprite dimensions: {image_path} {before.size}")
            before_hash = sha256(image_path)
            repaired = before.copy()
            repaired.putalpha(mask)
            repaired.save(image_path, optimize=True)
            after_pixels = np.asarray(Image.open(image_path).convert("RGBA"))
            if not np.array_equal(before_pixels[:, :, :3], after_pixels[:, :, :3]):
                raise RuntimeError(f"RGB changed while repairing {image_path}")
            entries.append({
                "file": image_path.name,
                "mask": str(mask_path.relative_to(STORY_DIR)),
                "before_sha256": before_hash,
                "after_sha256": sha256(image_path),
                "rgb_preserved": True,
            })
            preview = repaired.resize((256, 384), Image.Resampling.LANCZOS)
            background = (28, 31, 35) if column % 2 == 0 else (238, 238, 234)
            composed = Image.new("RGBA", preview.size, background + (255,))
            composed.alpha_composite(preview)
            variants_proof.paste(composed.convert("RGB"), (column * 256, row * 384))
            variants_draw.text(
                (column * 256 + 6, row * 384 + 6),
                f"{selector}/{image_path.stem}",
                fill=(255, 70, 70),
            )

        neutral = Image.open(directory / "main.png").convert("RGBA")
        for column, background in enumerate(((28, 31, 35), (238, 238, 234))):
            composed = Image.new("RGBA", neutral.size, background + (255,))
            composed.alpha_composite(neutral)
            proof.paste(composed.convert("RGB"), (column * 512, row * 768))
        draw.text((8, row * 768 + 8), selector, fill=(255, 70, 70))
        neutral_mask_path = MASKS_DIR / f"{selector}__main.png"
        alpha = np.asarray(Image.open(neutral_mask_path).convert("L"))
        report["characters"][selector] = {
            "neutral_mask": str(neutral_mask_path.relative_to(STORY_DIR)),
            "alpha_min": int(alpha.min()),
            "alpha_max": int(alpha.max()),
            "alpha_mean": float(alpha.mean()),
            "variants": entries,
        }

    proof.save(ART_DIR / "dark-light-proof.png", optimize=True)
    variants_proof.save(ART_DIR / "all-variants-proof.png", optimize=True)
    (ART_DIR / "repair-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
