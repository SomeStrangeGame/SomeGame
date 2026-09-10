## 2026-09-08T05:59:30Z — catalog-resume-reset — ready-for-review

Task: Fix missing Continue and undiscoverable reset in the fallback catalog.
Cause: episode ActionLabel always used OpenContent for unfinished playable episodes;
restart was an unlabelled conditional icon.
Changed: CatalogFlow uses SaveChoice presence for Continue; Card exposes a labelled
restart button on unlocked episodes (disabled without progress); prefab adds its
caption and disabled opacity. Completed/locked states and dependent reset semantics
are unchanged. Expanded existing live validation covers resume/reset pairing,
unread disabled reset, both drag axes, and open/cancel without confirming reset.
Validation: catalog content-gate 20260908T055513Z and Novels editor-gate
20260908T055611Z passed; fresh helper compile had zero errors. Live check passed:
two stories, continueButtons=1; saved Tzm shows Continue and Start Again; confirmation
warns about this and six following episodes, cancellation was visually verified.
Evidence: Novels/Assets/Build/Logs/catalog-continue-reset-20260908.png and
catalog-reset-confirmation-20260908.png. Editor left playing on the Tzm card;
confirmation closed, no user save reset. Recovery scene copy preserved under
Novels/Build/Logs/automation/catalog-resume-reset-recovery-20260908.backup.
Pending: user visual review; no Android/tablet run, commit or publish.

Hold-confirmation implementation and validation preserved in
[`2026-09-08 catalog hold history`](CoordinationHandoffHistory-2026-09-08-catalog-hold.md).

## 2026-09-08T06:24:00Z — catalog-locked-description — ready-for-review

Changed: CatalogFlow replaces locked-episode synopsis with «Чтобы открыть этот
эпизод, дочитайте предыдущие эпизоды.»; unlocked descriptions/unlock logic unchanged.
Validation: fresh live Novels compile, zero new errors; Check Live Catalog passed
for all 16 locked descriptions across zdm/tzm, including text height and both drag axes.
Evidence: Novels/Assets/Build/Logs/catalog-locked-description-20260908.png inspected.
Editor left playing on zdm episode 2; no progress changes, content rebuild or publish.
Existing validation helper now checks the prerequisite text and can show a locked card.
