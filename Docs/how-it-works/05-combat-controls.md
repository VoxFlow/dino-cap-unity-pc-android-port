# 5. Combat controls: melee, shooting and the "held button" rules

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The game has a melee button and a fire button. It does not remember taps and replay them
> later. It remembers only whether each button is *currently held*. That single rule explains many details players
> notice: pressing melee again during a swing and holding it until the swing ends starts another swing by itself, a bite
> does not cancel a held button, and a quick tap during a swing does nothing. Merely holding the one press that started
> the swing does not repeat it.

## 5.1 The rules, as recovered from the original program

1. Pressing a button sets its **held flag** and records which finger or mouse button owns it. Letting go clears the
   flag and forgets that finger.
2. A melee press that is accepted clears the melee flag as it starts the swing (so holding from the very start does
   not repeat). A *rejected* press (for example during another swing) leaves the flag alone.
3. There is **no queue** and **no melee cooldown**.
4. When a swing finishes, the game looks at the flags: if melee is held again (a new press made during the swing,
   which was rejected as an action but re-armed the flag) it starts another swing; otherwise if fire is held it fires.
5. After a bite, when the hurt animation ends, the same check runs: held melee restarts melee, held fire resumes
   firing.
6. Switching weapon by hand clears both flags (the current swing still finishes). Automatic switching when ammo runs
   out does not.
7. Pausing clears the flags.

Behaviours confirmed by the owner on an original iPad: holding Blue mid-swing past its end repeats the swing with no
walking until release; a bite while still holding gives another swing after the hurt animation; double-tap-then-hold
works; holding from the first instant does not repeat after a bite; Pause then Back cancels.

## 5.2 The "rage" face

The player's face changes while a heavy weapon is fired: after 10, 30 and 50 shots the face switches through three
angrier versions. The face is not a state machine but a small *suffix* added to the head picture name. It returns to
normal through stackable two-second "fireup" timers that are created when a fire run ends and on every weapon equip.
It is erased immediately only if fire was active when the player is hurt, enters a house, or starts a melee swing.
The hurt clip uses its own face, so the angry face returns afterwards until the timer fires.

## 5.3 Doors

The door icon appears when the horizontal distance between the player and the house's "md" child object (`com2`, not the
door leaf) is less than that child's width times 0.7 plus 5. The formula is proven from the original code; that the
child's reconstructed world position matches is LIKELY. The icon itself acts when a touch is *released* inside it, not when pressed.

## 5.4 Known original quirks kept on purpose

- Turning away from a world edge the camera is pinned to makes the camera jump one frame (original bug, reproduced on
  iPad and Android; kept).
- Bazooka firing overlapping the house enter animation (original iPad bug; kept).

## 5.5 Evidence pointers

- Native functions: melee press [`sub_666BC`](../technical/functions/dossiers/input-combat/FUN-0666BC.md) (0x666BC), completion dispatcher [`sub_66550`](../technical/functions/dossiers/input-combat/FUN-066550.md), touch handler [`sub_56E94`](../technical/functions/dossiers/pause-time/FUN-056E94.md), hurt entry [`sub_68148`](../technical/functions/dossiers/input-combat/FUN-068148.md), door gate [`sub_CE2C`](../technical/functions/dossiers/doors-buildings/FUN-00CE2C.md); all in the [address index](../technical/functions/ADDRESS_INDEX.md).
- Registry: `citygrind.melee_ranged.arbitration_no_queue_melee_wins`,
  `player.melee_flag.survives_bite_and_restarts_melee_after_hurt`, `player.manual_weapon_switch.clears_melee_and_fire_flags`,
  `citygrind.rage_face.native_fireup_timer_and_erase_events`, `citygrind.house.door_icon_*`,
  `camera.follow.one_frame_blip_when_turning_away_from_clamped_edge`.
- Superseded earlier claims (kept as history with notices): Task027L.1, Task023 and several N2.49R/N2.51 files.
