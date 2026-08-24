# Parallel work: character alpha trim

Status: completed, manual visual acceptance pending.

## Scope

- runtime geometry restoration for trimmed character layers;
- one generated trim manifest per story;
- Editor/CLI report and apply commands;
- all eligible TZM/ZDM body, clothes, emotion, hair and accessory PNGs;
- Android/iOS rebuild and size comparison.

Catalog files were not changed.

## Result

- TZM: 435 PNG, aggregate useful-area reduction 80.40%, source saving
  23 878 030 B;
- ZDM: 455 PNG, aggregate useful-area reduction 80.98%, source saving
  66 291 938 B;
- repeated report: 0 new trims, all 890 assets recognized as already processed;
- Android bundles: TZM 302 878 587 B, ZDM 102 707 947 B;
- iOS bundles: TZM 179 906 204 B, ZDM 60 588 188 B.

Original PNG and `.meta` files are recoverable from:

- `Projects/novels-tzm/Build/SpriteTrimBackup/20260824T135150Z`;
- `Projects/novels-tzm/Build/SpriteTrimBackup/20260824T141330Z`;
- `Projects/novels-zdm/Build/SpriteTrimBackup/20260824T135208Z`.
- `Projects/novels-zdm/Build/SpriteTrimBackup/20260824T141349Z`.

## Validation

- Unity batch compile of the main Novels project: passed;
- `trim-sprites all report 4`: passed and idempotent;
- `validate tzm`, `validate zdm`: passed;
- Android/iOS content builds and bundle audits: passed;
- `git diff --check`: required as the final textual check.

## Manual acceptance

Open representative characters on narrow and wide screens and compare body,
clothes, emotion, front/back hair and accessories with the backup originals. Automated checks
prove dimensions, addressing and compilation, but do not replace the visual
quality/alignment gate.
