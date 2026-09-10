# Story design

Read this reference for every new story.

## Author brief

Record the author's own genre wording verbatim. Keep it distinct from factual
basis. Also establish audience, tone, themes, prohibited material, expected
play time, linear or branching form, and the desired degree of player agency.
Ask only for missing decisions that materially change the result.

Genre is never inferred. In auto-approve mode, scope and ordinary creative
details may be inferred, but the author must still supply the genre.

## Narrative package

Create a compact package before implementation:

- logline and promise to the player;
- synopsis and central conflict;
- setting rules and continuity constraints;
- cast with goals, relationships, and emotional arcs;
- scene sequence with location, time, dramatic purpose, participants, costume,
  emotion, player choice, consequence, and required media;
- ending conditions and state that must survive choices.

For an episodic story, include each episode's local objective, discovery or
emotional turn, ending beat, next opening, target reading time and carried
state. Keep the complete reader-facing script separate from production notes;
when the author asks to see a new script, provide the actual revised scenes
and alternatives, not only a synopsis, plan or change list. Preserve the prior
draft and label literary draft, approved scenario and implemented Ink honestly.

Choices should express meaningful perspective, risk, relationship, knowledge,
or consequence. Do not add cosmetic choices solely to increase branch count.
Prefer reconvergence when branches do not justify duplicated art and prose.

## Reader orientation and explanatory prose

Write for a reader who has not seen the brief or previous editorial discussion.
Early scenes should establish who the protagonist is, their important
relationships, what kind of place this is, why they are here now and what they
want. Introduce supporting characters through their role and relationship,
not just a name. Make elapsed time, flashbacks, travel and location changes
explicit enough to follow without the production notes.

Do not substitute atmosphere for essential facts. For example, when a person
is missing, distinguish the disappearance, last confirmed sighting, elapsed
time, what was searched and what remains unknown. Evidence of an earlier
departure is not evidence of the person's present safety. Apply the same
distinction between fact, hypothesis and confirmation to other central mysteries.

Let readers form a guess, then confirm important correct deductions in the
text once the viewpoint character has grounds to do so. Explain causal links
and consequences even when they may seem obvious to the author. Deliberate
uncertainty can serve the story; accidental uncertainty about who, where,
when, why or what just happened does not.

Allow generous description, everyday detail, memories, sensory impressions,
hesitation and pauses. They give readers time to inhabit a place and understand
a reaction; they are not automatically expendable "padding". Prefer this
breathing room to compressed plot summaries or strings of cryptic aphorisms.
Avoid repeating the same explanation unchanged merely to lengthen a scene.

For a consequential action, show what the character wants or fears, why this
course seems plausible to them now, and what it costs. Ground relationships in
specific shared experiences and ordinary behaviour. A confession or stated
motive does not replace seeing the character act, hesitate, justify a choice
or live with its effects. Understanding an antagonist's motive is not the
same as excusing it; helping someone need not erase anger or imply forgiveness.

## Explain the extraordinary through the plot

When supernatural entities, anomalous objects or unfamiliar systems affect
decisions, establish what they are beyond their appearance. As relevant to
the story, reveal their origin, connection to the place or people, agency or
motives, capabilities, triggers, limits and ways to resist or end their effects.
Distinguish a being that chooses from a mechanism that follows a rule, and both
from a human deliberately exploiting that rule.

Distribute explanations through discoveries, records, remembered incidents,
conflicting accounts, dialogue and observable consequences. Tie each revelation
to the current need of the characters, then let it change their interpretation
or action. Do not stop the story for an unrelated encyclopedia entry. When
several phenomena share a cause, make that connection legible; do not declare
unrelated entities identical merely to simplify the lore.

Keep the viewpoint's knowledge honest: a legend or interested witness is an
account, not automatically established truth. Corroborate decisive explanations
within the fiction where needed. Preserve purposeful mysteries, but do not
use "nobody knows" to avoid explaining rules required to understand a choice
or ending. Foreshadow important exceptions rather than inventing a rescue rule
only at the moment it is needed.

Track separate causes and remedies. Relief from one effect need not grant
immunity to another; stopping a mechanism need not remove existing harm or
prevent a restart. Make such distinctions concrete before choices depend on
them, and keep them consistent across every branch and ending.

## Story notifications

Plan useful notifications in the literary script using the existing story
notification system. Place short messages at meaningful time/location changes,
confirmed discoveries and actual decision consequences. Keep them after the
supporting event and inside the branch that earned the knowledge; do not spoil
a later revelation or announce an unchosen outcome.

Notifications reinforce narration rather than carry an essential explanation
on their own. Use them selectively, not after every paragraph. They must not
invent achievements, scores, inventory items or interface capabilities. Follow
`Docs/AI/guides/InkSyntax.md` when translating them into commands; preserve the
narrative line needed before a choice rather than treating the notification
as its dialogue anchor. Literary condition labels are not runtime commands.

## Episodes and reading time

For an episodic visual novel without another requested pacing target, propose
15–20 minutes per episode. Respect a different author target, audience or
format. Choose the number of episodes to fit the approved story; four episodes,
five choices and three endings from one example are not universal requirements.
If meeting the target would materially expand the brief, agree the scope first.

Build episodes around local dramatic arcs, not equal-sized cuts in a text file.
Each should orient the reader, develop a question or task, deliver a meaningful
change and end on a fitting resolution, discovery or reason to continue. A
cliffhanger is useful when earned, not mandatory for every genre. Expand lived
scenes, relationships and aftermath rather than postponing answers solely to
fill time. Give alternative endings sufficient space to show their human and
world consequences; do not make a less favourable route abruptly perfunctory.

Resume from the prior ending with clear elapsed time and location. Use brief
recaps based only on that player's route, without replaying the whole episode
or importing discoveries from an unread branch. Distinguish the historical
choice from evolving knowledge or condition: an item not read when selected
may be read later, and relief from one effect may coexist with another.
Record these separate state facts in the handoff; neither an episode boundary
nor reconvergence silently resets them.

Estimate duration from text actually read on a selected path, excluding other
alternatives and author notes. Check short and long paths, including each
ending. State the assumed reading speed and allowance for decisions and
presentation; do not choose assumptions merely to make an oversized draft fit.
Report estimates as estimates and confirm pacing in the reading interface when
that validation is in scope. Do not call a word-count calculation a playtest.

## Scenario review before handoff

Read for comprehension without relying on the outline: can a new reader explain
the situation, the characters' immediate motives, the causal chain and what is
known versus suspected? Check that revelations precede decisions relying on
them, descriptions and emotional aftermath survived revision, and dialogue is
not merely a disguised summary of the setting rules.

Review transitions, recaps and notifications against carried knowledge and
conditions. Enumerate feasible choice combinations when tractable; otherwise
cover materially different and boundary routes. Check intended reachability,
no leaks from unread branches, consistent effects/remedies and the correct
ending for each route. Keep literary checks, Ink compilation, save/resume tests
and measured reading time separate in the evidence; claim only those performed.

## Checkpoints

In guided mode, obtain approval for the narrative package before full prose and
for the scene/asset manifest before art production. In auto-approve mode, keep
the same artifacts as reviewable evidence without pausing unless a stopping
condition from `SKILL.md` occurs.

## Narrative-originality criteria

Apply `Docs/AI/rules/OriginalityReviewProtocol.md`. For narrative, extract
compact fingerprints from distinctive wording, unusual cast/relationship
configurations, specific event chains, setting devices, choice/consequence
structures and endings. Compare expression and the selection, arrangement and
sequence of concrete elements—not isolated tropes, archetypes, broad premises,
facts or necessary chronology.

A targeted narrative revision may change causal structure, motivations,
relationships, scene order, player agency, consequences, setting mechanics or
resolution while preserving approved author decisions. The shared protocol
exclusively owns iteration count, evidence, risk levels, provenance and
fail-closed behavior.
