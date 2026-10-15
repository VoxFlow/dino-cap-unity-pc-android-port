# Corrected claims: what was believed, what disproved it, and what is true now

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** Many early conclusions turned out wrong and were fixed. This page lists the important ones
> honestly. The aim is not to hide mistakes: old reports are kept with a notice that points to the current answer
> (a banner at the top, or a notice key in JSON files), so nobody builds on a stale claim. One limitation is stated below.

How to read the status column (one status per claim): **FALSIFIED** = disproved; **SUPERSEDED** = replaced by a
better-supported claim; **REFINED** = partly right, narrowed. Successive corrections, heuristic rejection (row 9) and task reopening (row 10) are described in the text of the row, not in the status.

| # | Old claim (where) | What disproved it | Current answer | Status |
|---|---|---|---|---|
| 1 | Dinosaurs walk in a fixed direction (Task005) | A vector-instruction (NEON) misread | Dinosaurs home on the player and re-sign velocity each tick | FALSIFIED |
| 2 | Early dino field meanings and respawn slope −0.35, multiplier (1.0, 0.65, 0.30) (Task002 Phases A/C) | Parser evidence and ARM review | EXP / ATK / ADIS fields; slope −0.05; multiplier (1.0, 0.95, 0.90) | SUPERSEDED |
| 3 | Level clears by entering the house (Task002 Phase D) | Task006 | Clear trigger is positional (reaching the end of the street) | SUPERSEDED |
| 4 | Health resets to 100 on a normal clear (Task002) | Task007 | A normal clear carries health over; a fresh GO start and a death replay restore full health | SUPERSEDED |
| 5 | Animation timing tables (Task012) | Task013, then Task014's exact 97-call ledger | Task014 ledger is canonical (corrected in two successive passes) | SUPERSEDED |
| 6 | Event-mark inventory (Task016) | Bounds validation in Task018 | Task018 effective table | SUPERSEDED |
| 7 | Rusher bites from far away; phone and iPad numbers mixed (Unity "V10", Task024D) | Visual comparison with the original and a space mix-up | iPad gameplay constants only; bite contact on frame 12 of the bite, with a live re-check | FALSIFIED |
| 8 | Camera speed 195 on iPad and 205 as a "shipped quirk" (Task024E) | Native registration proof in N2.55 | iPad pan 315, phone 195 | SUPERSEDED |
| 9 | "Sidewalk" height −181, later a calibrated 82 (N2.41/N2.42) | Original video, then the no-calibration rule (N2.43) | Real derivation from the original transform chain (N2.44–N2.48). The 82 value was also rejected as a heuristic | FALSIFIED |
| 10 | N2.51 "City whole-mode closure" is done | Device acceptance 0 of 32 | The closure assertion was wrong; the task was reopened and later series continued | FALSIFIED |
| 11 | Blocked melee-bound shot consumed on a release edge or when melee ends (Task027L.1 proposals, V30.1–V30.4) | Native touch-owned flags sampled at swing completion | No deferred queue: a press made during a swing is rejected but re-arms the held flag; keep it held through completion. A tap released during the swing does not survive (chapter 5) | SUPERSEDED |
| 12 | Melee function clears its flag even when the press is rejected (Task027L.1) | Branch at the rejected path skips the clearing store | Flag cleared only on success | FALSIFIED |
| 13 | Mid-swing re-tap re-swings (Task023) | Device test and ARM | Only a still-held button re-swings | REFINED |
| 14 | Pause is not a global clock stop (N2.51 row 4.16) | Time-scale proof | Pause is a scene time scale of 0 | FALSIFIED |
| 15 | Door button closed (N2.51 row 4.14) | Press vs release semantics missed | Acts on release inside its rectangle | REFINED |
| 16 | Queued deferred shot through melee and a 0.35 s melee cooldown (N2.49R/N2.51) | No such mechanism in the native code | No queue, no cooldown | FALSIFIED |
| 17 | Player vtable +36 is the secondary completion | Re-check of the table | +36 primary completion, +40 secondary | FALSIFIED |
| 18 | House enter/exit has no dedicated animation (registry) | Full container merge found | Entry plays frames 0–7, exit 3 to end | SUPERSEDED |
| 19 | Fire-queue and 0.1 s heuristic for the rage face reset | Native "fireup" timer model | Stackable 2.0 s timers plus specific erase events | SUPERSEDED |
| 20 | Door range as a fixed 120 | Native proximity gate | abs(player.x − com2.x) < com2.width × 0.7 + 5, where com2 is the house composite's child, not the door leaf. The formula is proven; the child-to-world-position reconstruction is LIKELY | SUPERSEDED |
| 21 | Universal nearest-neighbour filtering | Loader evidence and iPad screenshots | Mixed: one sheet linear, others nearest | SUPERSEDED |
| 22 | Walking spawn margin 200 on the left and 195 on the right | A float constant was decoded wrongly; the bytes are exactly 200 | 200 on **both** sides; the 195 reading was reverted (`n255.k4.audit.walk_margin200`) | FALSIFIED |
| 23 | The actor-associated thin line is a Unity raster artifact with no patch to restore it (Task025F), and earlier line theories (025D shadow rim, 025E ground rim) | Owner observation of lines on limbs and in mid-air on the original, plus N2.52 evidence that the dinosaur sheet is natively linear-sampled | Owner-observed original limb/floating line defects are kept at native 1X; no single common mechanism is claimed for all earlier line theories (fidelity page, section 3) | SUPERSEDED |

## Where the notices live

Several superseded reports received a banner or a JSON notice key on 2026-10-14; other reports already carried
correction sections inside the body. The registry entries carry `superseded_by` or `supersedes` links, and a hygiene pass
annotated three dangling links.

**Disclosed loss of history.** Not every old output survives intact: 185 historical report outputs were overwritten or
regenerated during a later build-establishment pass and the originals could not be recovered (see the build report and
`historical_overwrite_provenance.json`). Those files must not be treated as untouched evidence from the original run,
and current hashes cannot authenticate their earlier contents.

## Lessons the corrections taught

1. A number matching a test does not prove it means the right thing (items 9, 10).
2. A function's body does not prove when it runs (items 11, 12).
3. The decompiler's output is not authoritative for signs, widths and vector instructions (items 1, 22).
4. Structural twins (phone and iPad code) are not the same thing (items 7, 8).
5. A user comparison with the real device overrides an earlier "PROVEN" label.

The quality chapters expand on these.
