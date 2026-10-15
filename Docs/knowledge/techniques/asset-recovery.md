# Recovering animation and sprite formats

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

**Order:** recover the raw format and its runtime meaning, build a neutral lossless intermediate form, prove a byte-exact round trip, and only then convert for the engine.

**What worked (Dino Cap):** parse every file, rebuild the original bytes (99 of 99 animation files byte-identical), recover the atlas from the loader's hard-coded calls
(no shipped metadata file existed), and play the animation recipe with a player that follows the original rules (draw then advance, at most one frame per update, parts in stored order, parts bound by index).

**Pitfalls:** art file names need not match type numbers; a missing atlas entry may be intentional (drawn by code); do not hand-slice atlases or re-author animation in the engine's own animator.
Details: [how the game stores its art](../../how-it-works/02-how-the-game-stores-its-art.md).

