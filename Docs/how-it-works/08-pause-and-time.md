# 8. Pause and the flow of time

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** Pausing in the original does not freeze objects one by one. It sets the whole scene's clock to
> zero, so everything that runs on game time — dinosaurs, blood and gore particles, timers, physics — stops together,
> while the pause menu, which runs on real time, still animates.

## 8.1 How it works natively

The scene has a time-scale value. Pausing sets it to 0 and resuming sets it to 1. The scene update multiplies the
frame time by it and feeds that scaled time to timers, game objects and physics; the user interface uses the raw time.
That is why flying heads and casings stop mid-air in the original.

A touch that *begins* in the top-right 100 × 100 area of the 1024 × 768 screen pauses at once. The pause menu has seven
items built from the same control class as the main menu: they shrink to half size while pressed, and dragging off the
button cancels the press.

More details recovered from the original:

- Every interactive pause control plays the button click first, then acts.
- The dim backdrop has alpha 192/255.
- Choosing *Menu* does not unpause: the paused picture stays until the next scene has loaded (about a second).
- Pausing clears held melee and fire (chapter 5).

## 8.2 What the port does

All time-driven objects check a global pause flag, and the particle movers run after the main update so they see it.
Correction made late in the project: an earlier N2.51 claim that pause "is not a global clock stop" was falsified by
the time-scale proof. The old audit file now carries a supersession notice pointing to the current contract.

The port's pause started as a shared flag that froze animations and particles. Later checks found gameplay timers still ran on Unity's
wall clock (spawn intervals, hurt stun, damage immunity), so the City and Jungle now use a scene clock that stops while paused. The
blood-impact burst's lifetime also counts only unpaused time. These were checked by targeted headless tests, not yet on screen, and the
Sandbox (debug workbench) scene has not been audited for the same issue.

## 8.3 Open observation

The owner saw a walking-and-shooting pose "close its legs" when pausing in Unity while the iPad freezes mid-stride. Code
reading and two headless checks could not reproduce it; it is recorded as not pursued by user decision.

## 8.4 Evidence pointers

- Native functions: time-scale setter [`sub_17C950`](../technical/functions/dossiers/pause-time/FUN-17C950.md) (0x17C950) and scene update [`sub_17BA5C`](../technical/functions/dossiers/pause-time/FUN-17BA5C.md); touch handler [`sub_56E94`](../technical/functions/dossiers/pause-time/FUN-056E94.md).
Registry: `citygrind.pause.native_time_scale_zero_mechanism`, `citygrind.pause.moving_particles_freeze`,
`citygrind.pause_button.fires_on_touch_begin_in_native_zone`, `citygrind.pause_menu.*`,
`citygrind.pause.player_pose_freeze.user_report_not_reproduced`.
