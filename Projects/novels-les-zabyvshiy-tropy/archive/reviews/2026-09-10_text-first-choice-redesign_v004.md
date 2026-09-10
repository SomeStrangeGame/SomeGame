# Text-first choice redesign — 2026-09-10 — v004

Story: `les-zabyvshiy-tropy`

The author rejected the prior horizontal choice presentation because it preserved the child-story interaction model: two or three large pictures acted as the choices while their text was hidden. Adult visual styling on those cards did not correct the structural mismatch.

The replacement follows the established interaction pattern of `novels-chernaya-melnitsa` without copying its artwork. Choices are a vertical stack of wide `380x96` buttons. The label is always visible, uses a readable `20px` size, and receives the majority of the button width. The authored `choice_icon` sprite remains inside each button only as a compact `64x64` supporting thumbnail at the left.

The button surface reuses this story's selected adult dark cartographic dialogue sprite in sliced mode at an 8x pixels-per-unit multiplier. This preserves the story-local visual language and avoids introducing a second image-card frame.

Static result: the bounded audit passes all 72 routes and now fails closed if choices become horizontal, labels are hidden, the wide-button geometry changes, or thumbnails expand into image-first cards. Runtime status remains pending: the previous APK and choice screenshots are explicitly superseded for layout acceptance.
