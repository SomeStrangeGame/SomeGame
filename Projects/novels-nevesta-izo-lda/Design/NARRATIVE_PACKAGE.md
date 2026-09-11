# Narrative package — «Невеста изо льда»

## Approved brief

- Story ID: `nevesta-izo-lda`
- Series: «Ночелесье»
- Genre (verbatim): «мистика, драма и лёгкая романтика»
- Factual basis: wholly fictional.
- Audience: 16+; broad visual-novel audience; emotional-engagement pilot.
- Boundaries: no graphic violence, sexual coercion, self-harm romanticization, or real-world ritual claims.
- Scope: one 25–45 minute episode, five meaningful choices, three reachable endings, six speaking characters, twelve used environmental states. Every complete route targets 3,000–5,000 spoken/narrated words rather than relying on aggregate source length.
- Approval: auto-approved reversible creative decisions; publication and final Unity slot remain separate.

## New-skill adoption review — 2026-09-11

The author retained the approved one-episode 25–45 minute scope. The newer
15–20 minute episode guidance is a default proposal only and does not override
this explicit target. The literary pass reviewed the complete reader-facing
episode without relying on this package: orientation, relationships, causal
explanations, notification timing, branch knowledge and all three aftermaths.

- The opening now identifies Mira as the village hydrologist and winter
  ferryman, explains why she is measuring the first crossing that evening, and
  gives her an immediate conflict between schedule and honest safety evidence.
- Mira and Osip's abandoned house is grounded in repeated ordinary actions
  rather than only their former engagement and his later confession.
- The first observable memory phenomenon, the orchard instrument and both
  variants of the forged-signature finding receive confirmation only after
  supporting prose. The text continues to distinguish observation, inference,
  testimony and proof.
- Notifications reinforce confirmed discoveries and actual branch outcomes.
  They do not carry indispensable explanations, reveal unavailable choices or
  report another ending.
- Choice IDs, state variables, eligibility, scene order, media inventory and
  ending meanings remain unchanged. The edit is a prose/notification revision,
  not a structural redesign.

Notification plan: crossing time/place after the bell evidence; Leda's separate
selfhood after Mira's observation; the mother's instrument after discovery;
confessed versus documented forgery inside the earned branch; one notification
after each final choice; and one ending-specific consequence after its narrated
aftermath.

## Player promise and logline

One winter night, hydrologist and ferryman Mira frees a nameless woman from the ice. The rescue breaks a covenant that keeps the village safe through pledged memories held beneath the lake and renewed every twenty-seven winters. Before dawn, the player must decide whether to return Leda, rewrite the bargain by consent, or substitute Mira's own memory.

The promise is intimate rather than epic: the player learns what each person was willing to forget, then chooses what a community may demand from one human heart.

## Setting rules

1. Glass Mere stores voluntarily or forcibly surrendered memories as warm bubbles beneath the ice.
2. The village covenant renews every twenty-seven winters. Personal bargains can occur between renewals; their surrendered memories join the same lake-held knot. Losing a shared recollection removes its felt experience, not every fact about the other person. The flood/covenant began twenty-seven years ago; Osip's personal bargain was seven years ago, when Mira was twenty and he was twenty-four. Mira's mother survived until that spring and then died. Mira's letter at twelve was a precaution after her mother's warning, not evidence of an engagement or sacrifice at twelve.
3. Leda is not an ancient bride or resurrected victim. She is a new person assembled by the lake from fragments that never belonged to one life.
4. Mira's own refusal removes her name from the pact's consenting parties at the weir, on both confession/ledger paths. Osip remains responsible for forgery but cannot veto her refusal or Leda's freedom. Past memories are not automatically restored by that correction. A new shared promise requires the intact original ribbon and Leda's free agreement (`trust_leda >= 2 && ribbon_free`), not Osip's confession. Each participant speaks only for themselves; absent villagers are not bound by proxy.
5. Every final choice leads through the same `dawn_seal` knot before its outcome: settlement occurs at first light, never as punishment for a failed attempt. The Keeper checks proposals without charging them beforehand. An unavailable shared promise is explained but not offered as a selectable option; return and voluntary memory payment are always available. Mira prepares her explanatory letter before choosing and fills in her decision before settlement if she gives her memory. Fire cannot melt covenant ice; explicit changes to consent can. Returning Leda or substituting Mira's night's memory renews the old twenty-seven-year arrangement; only `thaw` replaces it with revisable voluntary testimony.
6. The old pact reclaims Leda by force if no replacement is agreed; it does not automatically take a random new memory. In `thaw`, future refusal cannot restore the old payment or trigger a memory/life penalty. Winter has only just begun; the thaw is supernatural, not the ordinary end of winter. The bell's tongue and cracked bronze are restored before its clean strike, and all five travelers explicitly leave the underwater hall.

## Cast and arcs

- **Mira** (`Мира`): 27, hydrologist and winter ferryman. Begins as a precise observer who treats inherited customs as data; learns that neutrality also chooses a side. Whole outfit `winter`, variants `main`, `tender`, `determined`.
- **Leda** (`Леда`): appears 25, newly self-aware being of frost and borrowed memory. Wants the right to become someone rather than repay other people's grief. Whole outfit `frost`, variants `main`, `hopeful`, `wrathful`.
- **Osip** (`Осип`): 31, carpenter and Mira's former fiancé. Seven years ago he signed for both of them to surrender their happiest shared memory and prolong Mira's mother's life until spring. He hid the forgery; disclosure is not forgiveness or restored romance. Whole outfit `winter`, variants `main`, `protective`, `remorseful`.
- **Agrafena** (`Аграфена`): 58, keeper of the covenant ledger and Mira's aunt. Defends the bargain because she remembers the winter before it, yet finally admits that inherited fear became authority. Whole outfit `keeper`, variants `main`, `commanding`, `compassionate`.
- **Savva** (`Савва`): 17, bell-ringer and courier. Carries sealed notes people write to selves who may forget. His courage turns communal secrecy into testimony. Whole outfit `courier`, variants `main`, `afraid`, `relieved`.
- **Keeper Below** (`Хранитель`): nonhuman custodian of the lake's equilibrium. Enforces the inherited form of the pact despite the lake's observable distinction between consent and coercion; recognizing that distinction is not yet agreeing to change the rule. Mira demonstrates the contradiction and proposes a replacement. Whole outfit `reeds`, variants `main`, `wrathful`, `merciful`.

## State graph

- `trust_leda` (0–3): taking Leda's hand, untying the ribbon and following her path each add one. Asking for documentary evidence does not reduce trust.
- `trust_village` (-1–3): calling witnesses, hearing Osip and following Savva each add one; cutting the ribbon subtracts one. Changes the square's response and whether living villagers answer Savva's invitation at the finale; does not authorize promises on their behalf or independently unlock `thaw`.
- `truth_osip` (bool): hearing the complete confession leads to public accountability at the weir; demanding the ledger reveals the forgery later while he still avoids responsibility. Alters village testimony and relationship dialogue, including `thaw`; never controls Mira's refusal or final-menu eligibility.
- `ribbon_free` (bool): whether the boundary ribbon is untied rather than cut.
- `promise_shared` (bool): whether the player chooses the eligible shared vow; routes `dawn_seal` to `thaw`.
- `memory_offered` (bool): whether Mira chooses the voluntary memory payment; routes `dawn_seal` to `lantern`. Both flags false means deliberate return and `glass`.

Five choices:

1. **Rescue method** — take Leda's hand / call for witnesses. Alters intimacy versus communal trust.
2. **Boundary** — untie the red ribbon / cut through it. Alters whether the old pact can be rewritten rather than merely broken.
3. **Osip's truth** — hear his full confession / demand the ledger first. Changes his public accountability, village response and relationship with Mira, not the availability of freedom.
4. **Route to island** — trust Leda's path / follow Savva's bell route. Alters trust and whose knowledge leads.
5. **Final payment** — shared promise when eligible / return Leda / offer Mira's memory. Two or three available options, with missing prerequisites explained first. All share one settlement moment.

Reachable endings:

- `thaw`: `trust_leda >= 2`, `ribbon_free`, choose shared promise. Six routes, including three ledger-first routes with `truth_osip == false`. Leda receives a mortal future; covenant becomes annual voluntary testimony. Unconfessed Osip is not automatically reconciled with Mira.
- `glass`: choose return Leda deliberately; no failed-shared route remains. Leda explicitly does not consent. Village is safe for another twenty-seven winters; Leda dissolves; Mira keeps responsibility for her choice and becomes ledger keeper.
- `lantern`: choose Mira's memory of this night only. Leda consents to live without owing affection or service; the old memories remain in the lake and she retains her own hours and journal. Mira remembers earlier years and Osip, but forgets tonight's rescue and revelations. She writes her own explanatory letter before payment; Savva delivers it afterward. The old covenant receives another twenty-seven winters, not a reform.

Current graph: 16 pre-final states, 38 complete selectable routes (6 `thaw`, 16 `glass`, 16 `lantern`), five decisions per route and 17 knots including the common settlement. The former 48-route baseline is superseded.

## Scene matrix

| # | Time / location | Purpose and agency | Cast / appearance | Required media |
|---|---|---|---|---|
| 1 | Blue hour, ferry landing | Establish measurements, impossible warmth, choice 1 | Mira main, Leda main/hopeful | `glass-mere-dusk`, `winter-breath`, `ice-whisper`, `rescue-hand` |
| 2 | Night, ferry house | Leda names herself; borrowed memory causes intimacy and unease | Mira tender, Leda hopeful | `ferry-house`, `hearth-under-ice` |
| 3 | Night, House of Letters | Mira reads her childhood warning; Leda records her first self-owned memories | Mira, Leda, Osip, Savva | `letter-house`, `hearth-under-ice` |
| 4 | Night, village square | Public cost; Agrafena invokes covenant; choice 2 | all humans; Agrafena commanding | `village-square`, `untie-ribbon`, `ribbon-snap` |
| 5 | Midnight, drowned orchard | Concrete cases expose edited memories; Mira's mother's measurements reveal consent as mechanism | Mira, Leda, Osip, Agrafena, Savva | `frozen-orchard`, `winter-breath` |
| 6 | Midnight, birch boundary | Leda hears memories; Osip confesses; choice 3 | Mira determined, Osip remorseful, Leda wrathful | `ribbon-grove`, `bell-shard`, `ice-crack` |
| 7 | Midnight, route to island | Choice 4 tests knowledge and loyalty | Mira, Leda, Savva afraid | `glass-mere-dusk`, `diverging-paths`, `distant-bell` |
| 8 | After midnight, old weir | Ensemble experiment proves the lake distinguishes command from consent; prior truth choice changes the evidence | all humans, Keeper voice | `old-weir`, `ice-whisper` |
| 9 | Before dawn, storm crossing and bell tower | Ledger proves Osip's seven-year-old forgery; Mira's refusal is recorded in both paths, his accountability differs | all; Agrafena compassionate, Keeper main | `covenant-storm`, `bell-tower`, `covenant-bed` |
| 10 | Below lake, before first light | Check admissible settlements, prepare letter, make choice 5, then common `dawn_seal` | all, Keeper wrathful/merciful | `under-ice-hall`, `shared-rowan`, `covenant-bell` |
| 11A | Sunrise | Thaw ending | Mira tender, Leda hopeful, Osip remorseful | `thaw-sunrise`, `thaw-theme` |
| 11B | Dawn | Glass ending | Mira determined, Keeper merciful | `sealed-dawn`, `empty-bell` |
| 11C | Dawn | Lantern ending | Leda hopeful, Savva relieved | `sealed-dawn`, `winter-breath` |

## Continuity and production constraints

- Leda never calls borrowed recollections her own after learning their source.
- The two-window room is a remembered drawing/hope, not an already built house. Mira recognizes her childhood handwriting; only the agreed recollections were lost.
- Village trust has visible information channels: the witness rescue, familiar route signals, and Osip's public confession through the seventh weir pipe. The gate's pulling chain is distinct from Mira's measuring chain. Leda agrees in advance to emergency closure if control is lost or the village is threatened; this agreement is honored without delaying rescue for another ritual question.
- Low-trust Leda withholds agreement before the menu; no shared-vow option is exposed on that path. Mira speaks for herself, not for the whole group; Osip's acknowledgment under the lake remains conditional on his confession route, but his silence cannot block the pact.
- Mira and Osip may reconcile honestly but the story does not promise romance as a reward.
- All new characters use whole sprites; no independently generated layers.
- Red ribbons mean recorded obligation, never a generic cult symbol.
- The bell is cracked before the story begins; it never rings cleanly until the `thaw` ending.
- Every added location must be used by an explicit `Локация:` selector in the episode; the expansion adds no character or expression variants.

## Narrative originality gate

- Current candidate status: `passed`, author-requested cycle 2, iteration 1 (2026-09-07); risk `low`, confidence `medium`. Covers the complete narrative body above, including the autonomy/deadline redesign; body SHA-256 `6210fc82e597cc9cd340d0ec1377f7de56b15c3634f84ec8dff1fe99e31ea3b2`. See [ORIGINALITY_REVIEW.md](ORIGINALITY_REVIEW.md) for the explicit new-cycle basis, five direct comparison sources, findings and limitations. The full-Ink result is recorded separately there and in `CONTENT_HANDOFF.md`; neither result is media or final acceptance.

### Historical iteration 4 — not current acceptance evidence

- Iteration 4, 2026-09-07: complete revised package above, including the prior chronology repairs and this pass's enforcement rule, informed emergency response, village information channels, conditional assent and explicit thaw exit. Supersedes iteration 3; the five choices, numerical state transitions and three ending conditions remain unchanged.
- Search basis: live compact-fragment/title/cast and descriptive motif searches, plus direct reading of Lafcadio Hearn's [Yuki-Onna](https://en.wikisource.org/wiki/Yuki-onna). Shared motifs are a winter woman, ferry-side setting and consequential promise; Hearn's plot centers on a secret identity, marriage and a prohibition on telling, not inherited memory accounting, forged consent or a community's contract revision. No dialogue or scene sequence was taken from it.
- Search limitations: broad search returned mostly incidental word overlap, not a verified phrase match. Two potentially relevant lake/keeper fiction pages could not be opened and their snippets were not treated as comparison evidence. No exhaustive corpus or uniqueness claim; the finding below rests on the complete candidate and direct comparison, not on absence of search hits.
- Iteration 4 repeated compact searches for the lake/contract/memory/consent/bride combination and the new hope-versus-consent phrase. Results remained broad motif overlap, not verified expression evidence; no new result is claimed as a fully reviewed work. The complete current candidate was compared again with the previously directly read Hearn text; no new substantive overlap was identified.
- Common elements excluded as non-findings: winter bride/ice woman, village bargain, memory sacrifice, bell tower, dawn deadline.
- Distinctive combination reviewed: a hydrologist-ferryman heroine; consent versus compliance as a measurable supernatural mechanism; a new person composing her first self-owned journal amid inherited memories; correspondence used as imperfect identity scaffolding; a drowned orchard preserving edited recollections; an old sluice experiment that teaches a nonhuman system to distinguish command from collective assent; annual testimony replacing sacrifice.
- Finding: no substantive matching expression, cast configuration, causal chain, choice graph, or ending set identified in the reviewed material. Risk `low`, confidence `medium` due to search limitations.
- Provenance: original fiction generated for this brief; no adaptation, homage, real persons, or licensed story source.
- Result: `passed`.
