# Glossary

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](disclaimer.md).*

| Term | Meaning |
|---|---|
| **A2B / C2B** | The original's animation files. A2B holds clips as lists of parts per frame; C2B is merged on top of an A2B (for example the house enter/exit motion). |
| **Atlas** | A big picture sheet holding many small images; a table says which rectangle is which picture. |
| **Contract** | One recovered rule about the original, stored in the registry with a confidence label and evidence. |
| **Registry** | `source_contract_registry.json`, the list of all contracts. |
| **PROVEN / LIKELY / POSSIBLE / UNRESOLVED / FALSIFIED** | Confidence labels. PROVEN applies only to the exact claim the evidence shows. |
| **Superseded** | An older claim replaced by a better one; kept as history with a notice. |
| **Native** | The original iOS program. |
| **Source-faithful** | Matches the original's behaviour. |
| **Platform adapter** | Glue code that lets a recovered rule run on PC/Android (for example mouse acting as a finger). |
| **Internal 1X / 2X** | Drawing the 1024 × 768 game picture at original size (1X) or at up to double (2X, a modern enhancement; on mobile capped to the display height). |
| **Smooth / Original (output)** | Soft versus hard-pixel scaling of the finished picture to your window. |
| **Widescreen expand** | Showing more world at the sides on wide screens (a port policy, not original). |
| **City Grind** | The street campaign mode. |
| **Jungle Blitz** | The survival mode. |
| **Held flag** | The game remembers whether melee or fire is currently held, not taps. |
| **Session (audio)** | One playing sound; the original allows 28 at once. |
| **Registry id** | A name like `audio.dino_death_sound_shares_the_28_voice_cap` used in evidence pointers. |
