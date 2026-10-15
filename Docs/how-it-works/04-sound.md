# 4. Sound

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The original game can only play a limited number of sounds at once (28 in total, music
> included). When a machine gun fires fast, the pile of sounds fills that limit and the next sounds are silently
> skipped until old ones are cleaned up. This is original behaviour and the port copies it, including the side effect
> that a dinosaur's death sound can occasionally go missing during sustained automatic fire.

## 4.1 One shared budget

The original audio manager keeps one counter of "sessions" (a session is one playing sound). A new sound is admitted
only while the counter is below **28**; otherwise the request is reported as successful but nothing is created. A
sound that has finished does not free its slot immediately: finished sounds are swept in a batch roughly every
**2.0 seconds** of real time. There is no reserved room for important sounds and no stealing of older voices.
Music and effects share the same 28 in the original.

Consequences, all consistent with what players hear on the original:

- Holding an automatic weapon (for example a minigun) fills the budget within a couple of seconds. Shots then
  disappear for a short while, until the sweep frees slots, then return. The pattern repeats at about two-second
  intervals.
- Every sound the game plays goes through the same call, including the standard dinosaur death sound
  (`Audio_death1`). A death that happens while the budget is full is therefore silent in the original too.

## 4.2 What the port does

All City Grind sound effects go through one gate (`NativeAudioSessionGate`) that reproduces the cap and sweep
behaviour: a 28-session cap, a 2.0-second sweep, over-cap requests dropped. The music is currently outside that gate, so
equivalence of the *total* budget is not claimed. The native mechanism rests on the retained Task027L.4.4 evidence. A
one-off diagnostic during this documentation (a minigun held for 16 seconds with a kill every half second) showed the
saturation pattern, but its raw results were not retained, so its figures are not used as evidence. The owner reviewed
the behaviour and decided to keep it unchanged.

Known small difference (not verified in the original's code): the original's looping music also occupies one of the 28
slots, so effects effectively have 27 while music plays. The port's music is outside the gate, so effects have 28.

## 4.3 Evidence pointers

- Native functions: sound call [`sub_7EB04`](../technical/functions/dossiers/audio/FUN-07EB04.md) (0x7EB04) and `AudioManager::Play` [`sub_165298`](../technical/functions/dossiers/pause-time/FUN-165298.md) (0x165298).
- Registry: `audio.dino_death_sound_shares_the_28_voice_cap`, `dino.death_sfx.per_variant_native_contract`.
- Report: Task027L.4.4 (audio manager session cap).
- Confidence: cap, sweep and shared routing PROVEN from the original code; the one-slot music difference is LIKELY.
