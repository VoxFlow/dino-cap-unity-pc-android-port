# 9. Main menu and other screens

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The menu is a set of sliding rows over a street scene where a black dinosaur silhouette
> occasionally walks by and roars. Choosing a level makes a banner slide in, the game shows a loading slide, and only
> then does it load the gameplay assets.

- **Level select and banner.** A half-black mask and an animated band slide in; the label and the "LEVEL" digits use the
  original geometry (band 1024 × 300, label 446 × 101). Rows slide in over 0.2 s with a 0.05 s settle.
- **Options.** Eight sliding rows; Back restores the primary buttons.
- **Menu dinosaur.** Every five seconds the menu attempts to activate a silhouette (an already-active one suppresses the
  attempt; the object is created once and reused). It walks at exactly 20 units per second from the left and
  deactivates once past x = 768. A roar plays on activation, even off-screen, if effects are on.
  A roar heard once after returning from the City is most likely this timer, not a leaked City sound (LIKELY,
  unreproduced).
- **Loading.** The original does not preload gameplay assets at launch; it loads them when the "Loading…" slide
  finishes sliding in.
- **Offline extras.** The list and achievement buttons keep their press feedback; no online service is emulated.

Evidence: registry `mainmenu.*`, `menu.*`, `platform.asset_loading.*`, `citygrind.n251.lifecycle_visual_menu.menu_*`,
`n253.menu.gait_and_offline_social_feedback`.
