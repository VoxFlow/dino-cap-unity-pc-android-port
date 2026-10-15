# 6. Dinosaurs: who they are, how they appear, how they attack

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** There are three kinds of common dinosaur in City Grind: a medium *blue* one, a fast but fragile
> *tan* one, and a slow, very tough one. Each kind has its own timer that decides when the next one appears. A dinosaur
> walks straight toward you; once you are inside its bite range it stops, plays one bite animation and hurts you on one
> specific frame of that animation. All of this comes from numbers and code in the original game.

## 6.1 Three kinds, and a naming trap

The stat file `dinosaur.prop` has one row per type (HP, SPD, EXP, FRQ, ATK, ADIS; chapter 2). The original's art files
are **not** numbered the same way as the types, so a file called `k1_move` is not "type 1". The spawner picks both the
stat row and the picture family in the same `switch(type)`, so there is no separate "skin" choice:

| Spawn type | Looks like | Art family | Role (phone / iPad values) |
|---|---|---|---|
| 1 | Blue | `k3` files | Common: HP 5, speed 50 / 77, appears most often |
| 2 | Tan / copper | `k1` files (sprite names `k4…`) | Rusher: HP 1, speed 200 / 308 |
| 3 | Slow tank | `k2` files | HP 25, speed 30 / 46, rarest |

Walking speed is the raw `SPD` number in world units per second. The player walks at a flat 205, so a rusher at 308 is
about one and a half times your speed.

## 6.2 Spawning

- One function creates "one dinosaur". It is driven by **three independent timers**, one per type, armed when the level
  starts. Each timer's next interval comes from a per-type formula that depends on level and difficulty
  (the full table was recovered from the code, including the later-corrected slope and multiplier values noted in the
  Task002 report).
- Walking dinosaurs appear just outside the camera: 200 units beyond the left edge or 200 units beyond the right edge
  (the same on both sides, in City Grind and Jungle Blitz). A short-lived reading of 195 on the right came from decoding
  a float constant wrongly (the bytes decode to exactly 200); it was reverted and is marked FALSIFIED.
- While the player is entering or leaving a building, spawning is suppressed.
- A dinosaur is removed when it ends up more than 500 units off-screen.

## 6.3 Chase and bite

Each dinosaur update does this, in this order:

1. Advance the current animation (this may fire the bite-contact frame or the animation-end).
2. If already biting, do nothing else.
3. Otherwise test the **attack condition first**. If true, start the bite: stop completely, keep facing the player,
   play the bite animation once.
4. If false, chase: set velocity toward the player at the type's speed, facing the direction of travel.

The attack condition is a strict cone: the player must be in front of the dinosaur and closer than its attack distance
(`ADIS` plus a random 0–19 added once at spawn). Exactly zero or exactly equal distance does not count. The damage is
applied on **frame 12** of the bite for all three types, after re-checking with the live positions, so stepping back
during the wind-up makes the bite miss. Being behind the dinosaur also misses.

Death has no falling state: the fatal-hit routine sets the state to "dead" and plays the death sound in the same call.
Airborne landing is a separate mechanism.

## 6.4 Corrections along the way

- An early reading said dinosaurs walked in a fixed direction. That came from misreading vector instructions; they do
  home on the player (Task005 correction).
- Early guesses for what `dino[34]/[36]/[37]` meant were replaced by the parser evidence: EXP, ATK, ADIS.
- Respawn slope `-0.35` and multiplier `(1.0, 0.65, 0.30)` were wrong; the verified values are `-0.05` and
  `(1.0, 0.95, 0.90)`.

## 6.5 Evidence pointers

- Native functions: attack predicate [`sub_FE50`](../technical/functions/dossiers/dinosaurs-spawning/FUN-00FE50.md) (0xFE50) and the animation tick [`sub_76A20`](../technical/functions/dossiers/animation-rendering/FUN-076A20.md); see the [address index](../technical/functions/ADDRESS_INDEX.md).
- Reports: Task002, Task005, Task024A, Task024B, N2.32–N2.34 (spawn gating, lifecycle, despawn).
- Registry: `n255.k4.audit.walk_margin200` (current; the older id `n255.spawn.asymmetric_walk_margins` is the falsified 195 reading), `dino.offscreen_removal_margin`, `citygrind.dino.death_audio_synchronous_no_falling_state`,
  `dino.death_sfx.per_variant_native_contract`, `citygrind.building.player_entry_exit_no_sprite_level_presentation_either`.
- Unresolved/open: the original-device impression that fewer dinosaurs are on screen than the model predicts is
  tracked in the quality chapters, not closed here.
