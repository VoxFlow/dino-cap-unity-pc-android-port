# 7. Saves, levels, lives and the shop

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The game keeps one small save file with your level, money, health, lives and weapon upgrades.
> You advance a level only when you *leave the shop*, not when you enter it, so quitting inside the shop puts you back
> at the level you just cleared. Dying restores the state saved at the start of that level.

## 7.1 The save file

The original writes one raw binary file, `dinocap.sav`, with fixed positions:

| Offset | Meaning |
|---|---|
| 0 | Highest level reached |
| 4 | Current level |
| 8 | Money |
| 12 | Player health (a decimal number) |
| 16 | Lives |
| after | Weapon tier and ammo data |

That is the **original** format (registry `save_file.schema.dinocap_sav`). The port does **not** use it: see 7.6.

## 7.2 Starting a run

Choosing a level and pressing GO starts from a *checkpoint*: health 100, money 0, lives 5, the weapon table reloaded
(weapon A and G at tier 0, others unowned, A with 200 ammo, A equipped), and one batch of weapon upgrades for every five
levels of the chosen level.

## 7.3 Clearing a level

Level clear is **positional**: it triggers when the player walks past a fixed point near the end of the street (about
x = 7342 on iPad), not by killing a quota and not by entering any optional bonus house. A normal clear does **not** refill health (an early guess that it reset to 100 was corrected by Task007). Health is
full at a fresh GO start and after a death replay. The
level number increases, the highest-level mark updates and the game saves **when you leave the shop**, after the fade.
Walking out of the shop on the right starts the fade; you keep walking at normal speed while it fades.

## 7.4 The shop

Purchases are explicit button presses: each computes a price, checks you can afford it, and plays a rejection sound if
not. Weapon icons come from the original's own icon sheet. Prices and numbers are drawn with the game's real font
rather than a number-sprite system, as an early pass wrongly assumed. Leaving the shop is what commits
the save.

## 7.5 Death and lives

Dying subtracts one life, then the saved state is restored and the level replays with full health and the opening
banner. When lives run out the game returns to the menu. Exiting through the pause menu does not write the campaign
checkpoint.

## 7.6 What the port does differently (platform adapter)

The port stores typed key/value entries as JSON in a file named `dinocap-save.json`, with atomic replacement and backup
recovery, a one-time read-only import of old preference values, and a reset-all option. Saves are therefore **not
byte-compatible** with the original. The original checkpoint rules above (what is stored, when it is written, what
death restores) are preserved on top of this storage format. All builds include debug tools behind a master switch that defaults to OFF; a separate "affects saves" option defaults
to ON, so debug level skips can change saved progression once the master switch is on. Native death rollback, consumed
lives on debug skips and the final-life Resume rules are preserved.

## 7.7 Evidence pointers

- Native function: shop-exit handler [`sub_6F124`](../technical/functions/dossiers/saves-shop-progression/FUN-06F124.md) (0x6F124).
Registry: `save_file.schema.dinocap_sav`, `citygrind.level_increment.save_timing`, `player.lives_counter.dword_21bdfc`,
`citygrind.n251.lifecycle_visual_menu.checkpoint`, `n253.city.death_save_replay_and_pause_boundary`,
`n253.shop.pointer_purchase_and_exit_save`, `shop.price_text.real_font_not_numeral_sprite`. Reports: Task001/001B,
Task002, Task006, Task007, N2.53.
