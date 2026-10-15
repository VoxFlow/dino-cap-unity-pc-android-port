# F3 Anatomical X-ray Skeleton assets (Task027M.8.3)

## What this is

The F3 (laser / jiguang) electrocution reaction alternates a hit dinosaur
between its hurt pose and an **anatomical X-ray** (bones visible through the
body). This folder holds the assets that drive the X-ray phase.

## Source classification

| Element | Classification | Detail |
|---|---|---|
| Bone pixel art | **SOURCE_FAITHFUL** | The real Dino Cap `k*gt*` (`gt` = 骨头 / *gutou* / bone) skeleton parts. They ship in every original atlas (iPad `dino1.png` / `dino2.png`, phone `k1.png` / `k2.png`) but no recovered native code instantiates them — every `k*gt*Frame` string reference is inside the four atlas *builders* only. Extracted verbatim from the iPad hi-res atlases; no resampling; alpha preserved. A mild cool tint is applied to the bright bone texels so they read as "X-ray" rather than loose bones. |
| Alternation timing | **PLATFORM_ADAPTER / VISUAL_RECONSTRUCTION** | The HURT ↔ X-ray flip is driven by the existing `k*_be` frame-3 reaction event (the same event that owns the F3 DoT). The native trigger for the skeleton is **unresolved**. |
| Generic luminance-inversion shader (M8/M8.1) | **DEBUG_ONLY** | Retired; default off. It never showed bones. |

## Files

* `../Resources/DinoCapGenerated/Textures/f3xray_skeleton.png` — the **runtime**
  packed atlas (512×1024). 40 sprites: `xr_<rig>_<part>` (dark body shell +
  bones, as shipped) and `xrb_<rig>_<part>` (bones only, for the debug
  `XRAY SHELL: OFF` toggle). `<rig>` is `k3` (serves species k1/k3/k4) or `k2`.
  Registered as atlas entries in `combat_manifest.json`; loaded by
  `Power2DSourceTruthRenderer.BuildSprites()` like every other dino texture
  (code forces Point filter).
* `k1/ k2/ k3/ k4/` — per-species individual part PNGs, for inspection and
  future per-species divergence. **Not** loaded at runtime (k1/k3/k4 are pixel
  copies of the rig-A set; k2 is rig B).
* `f3xray_species_anatomy.json` — per-species part list + bone-coverage stats.
* `generate_f3xray_skeleton.py` — the generator (re-runnable; reads the
  Extracted atlases + `sprite_registry.json`, writes the packed texture, the
  per-species folders and the manifest-entry JSON).

## Runtime wiring

`Power2DSourceTruthRenderer.xraySkeleton` (bool). When set, `Draw()` swaps each
body part's sprite to its `xr_*` / `xrb_*` equivalent under the **identical**
`k*_be` part transform (same pivot, rotation, per-part offset, `flipX`,
`drawOffset` twitch, sort order). Missing swap sprite ⇒ living part kept
(never blank, never magenta). `CombatSandboxV2` sets it from the F3 X-ray phase;
it is presentation only — no damage / audio / AI / HP / collision / F2.
