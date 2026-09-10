# Horror ambience handoff

All audio is original, sample-free procedural synthesis at 44.1 kHz stereo PCM.
The package contains three restrained tonal beds and five sparse events. Beds
use multiple slowly varying partials rather than a single mains-adjacent hum;
events avoid harsh peaks and prolonged singing-metal resonance.

Planned checks before acceptance: non-zero frames, duration/channel/rate, peak
and mean level, seam/click inspection, and in-Player balance against dialogue.
No voice layer is used, so intelligibility and synthetic-voice provenance are
not applicable.

Static measurement passed for all eight files: stereo 44.1 kHz PCM, non-zero
frames, peaks between `0.0194` and `0.0757` full scale, no clipping. Loop seam
deltas are `0.00009` (dawn), `0.00043` (mill) and `0.00137` (forest); final
perceptual seam and small-speaker checks remain part of the authorized slot.
