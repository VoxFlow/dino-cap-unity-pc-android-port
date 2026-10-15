# Task index: what each investigation answered

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The project was done as many small, numbered investigations ("tasks"). Each one asked one
> question about the original game and wrote a report. The tables below give a grouped overview in plain language; the
> **Appendix** at the end lists every report folder by exact name so a reader can find where any rule came from. Reports marked † were later corrected in part; they stay in the record with
> a notice pointing to the current answer.

Naming: `Task0NN` = early/bounded tasks; `Task027…` = the long weapon, effects and City Grind series; `N2.xx` =
"Task027N.2.xx", the late City Grind and runtime-acceptance series. Full reports live in the private evidence archive;
the public pages quote the conclusions.

## A. Foundations: save, progression, worlds (Tasks 001–009)

| Task | What it answered |
|---|---|
| 001 / 001B | How the save file is loaded and which current level and startup values the game uses. |
| 002 † | City Grind progression: level inputs, dinosaur stat rows, respawn timers, difficulty multipliers. (Early guesses were corrected inside the report.) |
| 003 † | Jungle Blitz initial progression and stats. Later work superseded parts of it: the type-4 timer is "effectively disabled but reachable", and N2.54/N2.55 corrected the no-difficulty-effect and early time-scaling readings and the post-150 divisor (current: registry `jungle.spawn.native_difficulty_tail`). |
| 004 | Player and weapon combat contract: how the player's weapons work. |
| 005 † | Player movement and dinosaur AI interaction. (The "fixed direction" reading was a vector-instruction misread.) |
| 006 | How a City Grind street (world and buildings) is constructed; level clear is positional. |
| 007 | The shop/between-level loop; health is not reset on a normal clear. |
| 008 | Jungle Blitz world and full run lifecycle. |
| 009 | The "KV persistence" record used for stored values. |

## B. Animation and art formats (Tasks 010–020)

| Task | What it answered |
|---|---|
| 010 | Reconnaissance of the `.a2b` animation format. |
| 011 | How the original plays `.a2b`/`.c2b` clips and draws their parts. |
| 012 → 013 → 014 † | The animation timing table, corrected twice until an exact 97-call playback ledger closed it. |
| 015, 016 † , 018 | Animation event markers (sound/effects at specific frames), their exact inventory and bounds. |
| 017 | A lossless extractor and neutral data format for animations (99 of 99 files rebuilt byte-for-byte). |
| 019 | The sprite atlas: how names map to picture rectangles. |
| 020 | A reference renderer that rebuilds sprites end to end. |

## C. Weapons, effects, gore (Tasks 021–027M)

| Task | What it answered |
|---|---|
| 021 | Where each weapon attaches to the player's hand. |
| 022 | Which secondary animations and fire/hit/gore effects exist and who owns them. |
| 023 † | Blue (melee) combat; later refined on mid-swing re-taps. |
| 024A / 024B | Dinosaur identity (type ↔ stats ↔ art) and chase/bite contract. |
| 024D / 024E † | Source-space viewport and the camera's catch-up when the player turns; the iPad/phone speed labels were later corrected. |
| 025, 025B–025F | Thin lines, edge artifacts, shadow line and scaling quality at the picture level. |
| 026, 026B | Dinosaur hit reactions, blood and gore, and the player's hurt/knockback/invulnerability. |
| 027, 027B–027I | Weapon cadence, hand/muzzle geometry, weapon identity mapping, E/G/F weapons, raster quality, pixel-exact comparison with the original. |
| 027J, 027J1–J4 | The weapon visual matrix, G-tier special death, G melee audio, template supersession. |
| 027K, 027K1–K4 (+ sub-parts) | F-family weapons (jupan, flamethrower, burn effects), audio voice lifetimes and burn residue. |
| 027L, 027L.1–L.4.15 † | Melee/fire arbitration, deferred fire, the audio session cap, and the exact timing model of the original engine's tick. |
| 027M, 027M.1–M.8.4 | The F3 laser / x-ray weapon: look, cadence, blend modes, a trace tool that was written but never built or run, stun lifecycle, true anatomical x-ray. |

## D. All dinosaur types and rage (Task 027N.1–2.31)

| Task | What it answered |
|---|---|
| 027N.1.x | Edge-range melee on types 3 and 4, hit-reaction arbitration, light recoil, knockback and decapitation rules. |
| `Task027N_2_BloodAudioBiteEndDebugGodMode` | The parent task for blood audio, bite end and debug god mode that the numbered blood tasks followed. |
| 027N.2.1–2.21 | Blood: spurt contract, directional bursts, "qiangxue/daoxue" blood assets and who triggers them. |
| 027N.2.22–2.31 | Bite commit and auto-melee, muzzle flash, the rage face state machine, and debug tools. |

## E. City Grind as a whole (N2.32–N2.51)

| Task | What it answered |
|---|---|
| N2.32–N2.34 | Spawn gating, concurrency, spawn lifecycle and despawn. |
| N2.35 | Migration to a multi-dinosaur core. |
| N2.36 (+ .1–.9) | Rebuild of the City scene: visuals, buildings, exits, rewards, house re-arm, event callers, factory registry. |
| N2.37–N2.40 | Runtime acceptance fixes, a headless-test audit, and a render-capture harness. |
| N2.41–N2.42 † | Placement attempts; their "sidewalk" value was falsified against original video. |
| N2.43 | The rule that placement must come from source, not calibration. |
| N2.44–N2.48 | The real derivation of actor height from the original transform chain. |
| N2.47R / N2.49R / N2.50 | Persistent source-contract registry, whole-gameplay conformance, F3 intro/hold/release. |
| N2.51 † | Whole-mode City closure; its first "done" was reopened after device acceptance was 0 of 32. |

## F. Runtime acceptance and later work (N2.52–N2.55 and after)

| Task | What it answered |
|---|---|
| N2.52 | Menu and City runtime A/B causal closure. |
| N2.53 | The remaining City causal closure (menu gait, death/save, shop purchase). |
| N2.54 | Jungle Blitz restoration, with the broad run at 922 of 922. |
| N2.55 | Runtime contradictions in City and Jungle (including the camera-profile correction). |
| `DC1_Build_Establishment`, `DC1_Mono_Android_Compatibility_Preflight`, `DC1_IDA94_BasicPreflight` | Establishing the Windows and Android builds, their preflights, and an analysis-tool compatibility check. The build report also discloses 185 historical outputs that were overwritten. |
| `DeliveryTest`, `Policy_ParentTask_NoPrematureClosure_Merge` | A delivery-mechanism test and the merge of the parent-task completion policy (process, not game behaviour). |
| F2/F3 reinvestigation | The ongoing flamethrower and laser device-acceptance rows (parent still open). |
| Fixes after N2.55 | Pause as a time scale, melee/fire arbitration, rage-face timers, door range, camera blip, audio-cap review. |

*The grouped tables were written in plain language from the report folder names and headings, and cover ranges, not a
unique sentence per folder. Each † entry has a matching row in [corrected claims](corrected-claims.md).*

## Appendix: complete folder inventory (exact names)

Utilities and process folders are marked *(process)*. Headings are taken from each report; generic "Investigation Report" headings are shown as the folder name only. The grouped tables above are summaries by range, not one sentence per folder; many appendix cells say "(see report)".

| Report folder | Heading in the report |
|---|---|
| `DC1_Build_Establishment` *(process)* | DC1 Build Establishment — compact report (runtime acceptance PENDING) |
| `DC1_IDA94_BasicPreflight` *(process)* | IDA 9.4 basic compatibility preflight |
| `DC1_Mono_Android_Compatibility_Preflight` *(process)* | DC1 Windows / Android compatibility preflight |
| `DeliveryTest` *(process)* | (see report) |
| `F2F3_Reinvestigation` | (see report) |
| `Policy_ParentTask_NoPrematureClosure_Merge` *(process)* | (see report) |
| `TASK027N_2_51_CityGrindWholeModeClosure` | TASK027N.2.51 — City Grind Whole-Mode Native-Conformance / Integration Closure |
| `TASK027N_2_52_RuntimeAB_MenuCity_CausalClosure` | (see report) |
| `TASK027N_2_53_CityGrindRemainingCausalClosure` | (see report) |
| `TASK027N_2_54_JungleBlitzRestoration` | TASK027N.2.54 — Jungle Blitz restoration |
| `TASK027N_2_55_RuntimeContradictions_CityJungle` | TASK027N.2.55 - SEMI_COMPLETE_USER_RUNTIME_AB_PENDING |
| `Task001B_HighLevelStartupCompatibility` | Task 001B — Legacy High-Level Startup Compatibility Audit |
| `Task001_SaveLevelStartup` | Task 001 — Save Load → Current City Grind Level → Startup Consumers |
| `Task002_CityGrindProgression` | Task 002 — City Grind Progression Contract |
| `Task003_JungleBlitzProgression` | Task 003 — Jungle Blitz Progression Contract |
| `Task004_PlayerWeaponCombat` | Task 004 — Player + Weapon Combat Contract |
| `Task005_PlayerDinosaurInteraction` | Task 005 — Player Movement + Dinosaur AI / Combat Interaction Contract |
| `Task006_CityGrindWorldConstruction` | Task 006 — City Grind World / Stage Construction Contract |
| `Task007_UpgradeSceneLoop` | Task 007 — City Grind UpgradeScene / Between-Level Loop Contract |
| `Task008_JungleBlitzWorldLifecycle` | Task008 — Jungle Blitz World + Complete Run Lifecycle Contract |
| `Task009_KVPersistence` | Task009 — "KV Persistence" + `dinocap.l1` Record Contract |
| `Task010_A2BRecon` | Task010 — A2B Animation Format Reconnaissance |
| `Task011_A2BRuntimePlayer` | Task011 — A2B/C2B Runtime Animation Player + Draw Semantics |
| `Task012_AnimationTimingAndD` | Task012 — Complete Animation Playback-Parameter Table + D Consumer Closure |
| `Task013_AnimationTimingFinalClosure` | Task013 — Animation Timing Table Final Closure + D Exhaustive Consumer Check |
| `Task014_AnimationTimingExactLedger` | Task014 — Exact 97-Call Playback Ledger + Final Timing Closure |
| `Task015_AnimationEventContract` | Task015 — CAFrame Event Marker + Weapon Animation Event Contract |
| `Task016_AnimationEventInventory` | Task016 — Exact Runtime Animation Event-Mark Inventory |
| `Task017_A2BExtractor` | Task017 — Lossless A2B/C2B Extractor + Neutral Animation IR V1 |
| `Task018_EventMarkBounds` | Task018 — Event-Mark Bounds Validation + Effective Event Table Closure |
| `Task019_SpriteAtlasRegistry` | Task019 — Sprite Registry + Atlas Rectangle Source-Truth Reconstruction |
| `Task020_ReferenceRenderer` | Task020 — End-to-End Neutral Sprite Reconstruction + Reference Renderer V1 |
| `Task021_WeaponAttachment` | Task021 — Player Weapon Attachment Transform Closure |
| `Task022_SecondaryCombatEffects` | Task022 — Secondary Animation Composition + Fire / Hit / Gore Effect Ownership |
| `Task023_BlueMeleeCombatClosure` | Task023 — Blue Melee Combat Closure |
| `Task024A_CityDinoIdentityClosure` | Task024A — City Dinosaur Spawn → Stat Row → Visual Variant Identity Closure |
| `Task024B_CityChaseAttackClosure` | Task024B — City Dinosaur Chase, Attack‑Range & Bite Contract  (+ Task024C‑Lite Type‑4 Visual Identity) |
| `Task024D_SourceSpaceViewportClosure` | Task024D — Phone/iPad Source-Space, Actor Geometry & Modern Viewport Scaling Closure |
| `Task024E_CameraFlipCatchupClosure` | Task024E — Facing-Flip Camera Catch-Up & World-Stability Closure |
| `Task025B_FrameRectTexelOwnershipClosure` | Task025B — Shared Frame-Rect / Texel-Ownership Closure |
| `Task025C_SourceFramebufferSamplingClosure` | Task025C — Source Framebuffer Scaling, Boundary Sampling & Color-Space Closure |
| `Task025D_ShadowLineRegressionClosure` | Task025D — Shadow-Line Preservation / Source-Framebuffer Regression Closure |
| `Task025E_AuthenticShadowLineDisplayClosure` | Task025E — Authentic Shadow-Line Identity + Source-RT / Display-Presentation Closure |
| `Task025F_ActorLineUpscaleClosure` | Task025F — Actor-Associated Thin-Line Identity + Post-Framebuffer Upscale Quality Closure |
| `Task025_AtlasArtifactFirearmCasingClosure` | Task025 — Atlas Edge / Transparency Representation + Firearm Discharge Visual Artifact Closure |
| `Task026B_BlueDinoGoreRuntimeClosure` | Task026B — Blue Dino Reaction + Gore Spawn/Socket/Floor Closure |
| `Task026_DamageFeedbackGorePlayerHurtClosure` | Task026 — Dino Hit / Blood / Gore / Death + Player Hurt / Knockback / Invulnerability Closure |
| `Task027B_WeaponCadenceHandMuzzleClosure` | Task027B — A Cadence + Family Weapon/Hand Composition + Exact Muzzle Socket Closure |
| `Task027C_WeaponIdentityMappingClosure` | Task027C — Weapon Identity / Slot / C2B / Fire-Branch Reconciliation |
| `Task027D_EGFWeaponClosure` | Task027D — E/G Implementation + F Continuous-Weapon Closure |
| `Task027E_EGFinalRasterClosure` | Task027E — E/G Final Implementation Gates + Transform-Raster Quality Closure |
| `Task027F_ESettledMeatSmoothRasterClosure` | Task027F — E Settled Meat + Smooth-Mode Raster Closure |
| `Task027G_PresentationKernelRefClassification` | Task027G — Presentation Kernel Closure + Unresolved-Ref Classification |
| `Task027H_K1GoreSourceRasterClosure` | Task027H — K1 Gore Identity Recovery + Source-RT Jaggedness Root Cause |
| `Task027I_NativeUnityPixelRasterDifferential` | Task027I — Native vs Unity Pixel-Exact Sprite Raster Differential |
| `Task027J1_WeaponMatrixContradictionClosure` | (see report) |
| `Task027J2_GMeleeAudioContractClosure` | (see report) |
| `Task027J3_GSpecialFatalImplementationClosure` | (see report) |
| `Task027J4_ToufeiSupersessionPDTemplateClosure` | (see report) |
| `Task027J_WeaponVisualMatrixGTierClosure` | (see report) |
| `Task027K1_F1JupanPresentationSpawnClosure` | Task027K1 — F1 JUPAN PRESENTATION / SPAWN CLOSURE |
| `Task027K2_F2FlamethrowerBurnStateDeathClosure` | (see report) |
| `Task027K3_HuoyanBurnFXVisualClosure` | (see report) |
| `Task027K4_1_F2QuickTapCancelReleaseAudioClosure` | (see report) |
| `Task027K4_2_F2AudioVoiceLifetimeRareHuoyanResetClosure` | (see report) |
| `Task027K4_3_F2HeldAudioRapidFacingTargetContinuityClosure` | (see report) |
| `Task027K4_4_F2NativeAudioVoiceBusyFreeLifetimeClosure` | (see report) |
| `Task027K4_F2HuoyanLifecycleResidualBurnClosure` | (see report) |
| `Task027K_FFamilyGameplayEffectsClosure` | (see report) |
| `Task027L_1_MeleeBoundSemiAutoReleaseAndMeleeTransitionOrderingClosure` | (see report) |
| `Task027L_2_DeferredA1FireAndMeleeCompletionWeaponFlashClosure` | Task027L.2 — Deferred A1 Fire After Melee + Melee Completion / Weapon-Flash Render-Order Closure |
| `Task027L_3_F2MeleeInterruptVsNewPressSessionOwnershipClosure` | (see report) |
| `Task027L_4_10_NativeDrawKeyAndPower2DTickClosure` | (see report) |
| `Task027L_4_11_NSTimerMissedFireAndFixedTickClosure` | (see report) |
| `Task027L_4_12_HostCadenceVsTransientStallClosure` | (see report) |
| `Task027L_4_13_MeasuredDtNativeModeAndRateConversionClosure` | Task027L.4.13 — POWER2D MEASURED-DT NATIVE MODE + ARBITRARY-HOST RATE-CONVERSION FINAL CLOSURE |
| `Task027L_4_14_CadenceClassifierFinalConvergence` | Task027L.4.14 — POWER2D CADENCE-CLASSIFIER FINAL CONVERGENCE + AUTOMATIC CONTRADICTION-CLOSURE GATE |
| `Task027L_4_15_F2BurnFxSublayerSourceTruthClosure` | Task027L.4.15 — F2 BURN-FX SUBLAYER SOURCE-TRUTH CLOSURE |
| `Task027L_4_1_Power2DF2EventCompletionBridgeClosure` | (see report) |
| `Task027L_4_2_CompleteF2BurnAudioOwnershipVoiceLifetimeSpatialDeathClosure` | (see report) |
| `Task027L_4_3_AudioAtt3RequestToAudibleVoiceGateSourcePoolClosure` | (see report) |
| `Task027L_4_4_AudioMaxSessionsSessionGCSilentDropClosure` | (see report) |
| `Task027L_4_5_F2BurnAudioIdentityNativeCallerFinalConvergenceClosure` | (see report) |
| `Task027L_4_6_ShaoEventDispatchAudioAtt3RepeatOverlapClosure` | (see report) |
| `Task027L_4_7_And_WeaponTierCompletion` | (see report) |
| `Task027L_4_8_F2ResidualAndCDAutoMeleeArbitrationClosure` | Task027L.4.8 — F2 Post-Release Burnt-State Lifetime + 1–2 Residual Audio_att3 Closure + C/D Full-Auto Held-Fir |
| `Task027L_4_9_F2Att3CadenceAndFlameDrawOrder` | (see report) |
| `Task027L_4_F2ResidualBurnStateSurvivorStunAudioContinuityClosure` | (see report) |
| `Task027L_PlayerCombatActionArbitrationMeleePriorityStuckStateClosure` | (see report) |
| `Task027M_1_F3LaserSourceTruthAndF2SharedMechanism` | Task027M.1 — F3 LASER SOURCE-TRUTH RECONSTRUCTION + F2 SHARED-MECHANISM VS LOOKALIKE CLOSURE |
| `Task027M_2_F3XrayCadenceExportRuntimeClosure` | Task027M.2 — F3 XRAY REPRESENTATION + EXACT BE EVENT CADENCE + ASSET EXPORT + RUNTIME IMPLEMENTATION CLOSURE |
| `Task027M_3_F3XrayQuickTapFinalClosure` | Task027M.3 — F3 XRAY REPRESENTATION FINAL CLOSURE + QUICK-TAP FIRE SESSION / RELEASE-LATCH CLOSURE |
| `Task027M_4_F3XrayBlendModeRoutingClosure` | Task027M.4 — F3 XRAY EXACT BLEND-MODE WRITER / ROUTING CLOSURE |
| `Task027M_5_F3XrayNonActorOverlayClosure` | Task027M.5 — F3 XRAY NON-ACTOR OVERLAY / PARTICLE / CLIP-SWAP FINAL CLOSURE |
| `Task027M_6_F3XrayDynamicRenderStateClosure` | Task027M.6 — F3 XRAY DYNAMIC RENDER-STATE CLOSURE + POINT-FILTERING CONTRADICTION RECONCILIATION |
| `Task027M_7_1_F3NativeTraceDeliveryClosure` | Task027M.7.1 - F3NativeTrace Actual Tool Delivery + Source / Hook-Signature / Package Audit |
| `Task027M_7_2_F3NativeTraceImmutableDelivery` | Task027M.7.2 - F3NativeTrace Immutable Nested-Archive Delivery Closure |
| `Task027M_7_F3XrayNativeTraceTool` | Task027M.7 — F3 XRAY ORIGINAL-IPAD NATIVE TRACE TOOL + CAPTURE / DECODER / COMPARISON PIPELINE |
| `Task027M_8_1_F3XrayMagentaFailSafeClosure` | Task027M.8.1 — F3 X-ray Magenta Shader Failure + Missing DEBUG Control Repair + Fail-Safe Material Restoration |
| `Task027M_8_2_F3StunLifecycle_TrueXrayClosure` | (see report) |
| `Task027M_8_3_F3TrueAnatomicalXray` | (see report) |
| `Task027M_8_4_F3AudioCadence_XrayEntry` | (see report) |
| `Task027M_8_F3XrayVisualReconstructionClosure` | Task027M.8 — F3 X-ray Visual Reconstruction Closure |
| `Task027N_1_1_Type34_EdgeRangeMelee` | (see report) |
| `Task027N_1_2_Type3HitReactionArbitration` | (see report) |
| `Task027N_1_3_1_LightRecoilInvulnClosure` | (see report) |
| `Task027N_1_3_2_RealInputPostHurtMelee` | (see report) |
| `Task027N_1_3_Type3KnockbackDecapInvuln` | (see report) |
| `Task027N_1_AllDinoTypes_SpawnController` | (see report) |
| `Task027N_2_10_Qiangxue_RangedBloodBurstAssetClosure` | (see report) |
| `Task027N_2_11_QiangxueAnim761_ExactAssetAndRoutingClosure` | (see report) |
| `Task027N_2_12_Qiangxue761_StaticDeepRecovery_F1LiteralClosure` | (see report) |
| `Task027N_2_13_F1_ABCD_LiteralWeaponClass_AndPendingQxPolicyClosure` | (see report) |
| `Task027N_2_14_TrueRangedBloodBurstOwner_Recovery` | (see report) |
| `Task027N_2_15_sub7A370_RangedBurstTwinPathClosure` | (see report) |
| `Task027N_2_16_RandomizedHeadAttachedBloodBurst_VisualFingerprintClosure` | (see report) |
| `Task027N_2_17_HurtClipEmbeddedDaoxue_DataDrivenBurstClosure` | (see report) |
| `Task027N_2_18_VideoPixelFingerprint_DaoxueIdentityClosure` | (see report) |
| `Task027N_2_19_SameBuild_DaoxueInstantiationPathClosure` | (see report) |
| `Task027N_2_20_Qiangxue761_GetTextureAnimationOverrideClosure` | (see report) |
| `Task027N_2_21_QiangxueSingleInstance_BloodFamily_BiteReactionClosure` | (see report) |
| `Task027N_2_22_BiteCommitAutoMelee_ABCDWeaponAudioClosure` | (see report) |
| `Task027N_2_23_MuzzleFlash_RageExpression_HappyFaceClosure` | (see report) |
| `Task027N_2_24_RageRegressionIsolation_MovingMuzzleSocketClosure` | (see report) |
| `Task027N_2_25_RageStateMachine_DamageModifierClosure` | (see report) |
| `Task027N_2_26_RageVisibleDecayToNeutralClosure` | (see report) |
| `Task027N_2_27_RageReleaseToNeutral_VersionBannerClosure` | (see report) |
| `Task027N_2_28_RageDirectNeutralResetClosure` | (see report) |
| `Task027N_2_29_RageConcurrentStateMachine_NativeClosure` | (see report) |
| `Task027N_2_30_RageLogicalVsRenderedFace_ExpirationClosure` | (see report) |
| `Task027N_2_31_DebugInfiniteAmmo_AutoSpawn_UILayoutClosure` | (see report) |
| `Task027N_2_32_CityGrindSpawnGating_Concurrency_EntranceClosure` | (see report) |
| `Task027N_2_33_CityGrindSpawnLifecycle_ContradictionClosure` | (see report) |
| `Task027N_2_34_DinoLifecycle_Despawn_FreeList_Closure` | (see report) |
| `Task027N_2_35_MultiDinoCoreMigration_Phase1` | (see report) |
| `Task027N_2_36_1_CityGrindVisualBuildingNativeClosure` | TASK027N.2.36.1 — City Grind Visual + Building Native Closure |
| `Task027N_2_36_2_CityGrindResidualNativeClosure` | TASK027N.2.36.2 — City Grind Residual Native Closure |
| `Task027N_2_36_3_CityGrindExitRewardEdgeClosure` | (see report) |
| `Task027N_2_36_4_CityGrindStartupUnlock_SequenceLoaderClosure` | (see report) |
| `Task027N_2_36_5_CityGrindA2BTiming_RewardLifecycleClosure` | (see report) |
| `Task027N_2_36_6_CityGrindHouseRearm_RewardLifecycleClosure` | (see report) |
| `Task027N_2_36_7_CityGrindTaggedEvent_RewardCallerFinalClosure` | (see report) |
| `Task027N_2_36_8_CityGrindFactoryRegistry_PatternBClosure` | (see report) |
| `Task027N_2_36_9_CityGrind_AntiChurn_FinalStaticClosure` | TASK027N.2.36.9 — CityGrind Anti-Churn Final Static Closure |
| `Task027N_2_36_CityGrindScene_Reconstruction` | TASK027N.2.36 — CityGrindScene Reconstruction |
| `Task027N_2_37_CityGrindRuntimeAcceptanceFixes` | TASK027N.2.37 — City Grind Runtime Acceptance Fixes |
| `Task027N_2_38_CityGrindNativeGameplaySceneClosure` | TASK027N.2.38 — City Grind Native Gameplay-Scene Closure |
| `Task027N_2_39_HeadlessUnityAudit_NativeGapClosure` | TASK027N.2.39 — Headless Unity Audit + Test-Provenance Audit + N2.38 Native-Gap Closure |
| `Task027N_2_40_SpawnLifecycle_RenderCaptureClosure` | TASK027N.2.40 — City Grind Spawn-Lifecycle Final Closure + Render-Capture Harness |
| `Task027N_2_41_L1Survivability_FinalFramebufferCaptureClosure` | TASK027N.2.41 — L1 Survivability + Final-Framebuffer Capture Closure |
| `Task027N_2_42_VisualSemanticWorldLayerClosure` | (see report) |
| `Task027N_2_43_SourceTruthWorldYClosure` | (see report) |
| `Task027N_2_44_DrawTimeTransform_PhysicsScaleClosure` | (see report) |
| `Task027N_2_45_SpriteSync_DrawOwner_FinalClosure` | (see report) |
| `Task027N_2_46_MultipartChildTransform_FinalNativeClosure` | (see report) |
| `Task027N_2_47R_A2BFrameApply_SourceContractClosure` | (see report) |
| `Task027N_2_48_FinalActorY_SourceTruthPatchAndVisualValidation` | (see report) |
| `Task027N_2_49R_CityGrindWholeGameplayConformanceClosure` | (see report) |
| `Task027N_2_50_F3IntroHoldReleasePhaseStateMachineClosure` | (see report) |
| `Task027N_2_6_TransientSpurt_VirtualDispatch_OutputOrderingClosure` | (see report) |
| `Task027N_2_7_BloodVisualPhaseRepresentationClosure` | (see report) |
| `Task027N_2_8_BloodSpurtContract_GlobalGravityScopeClosure` | (see report) |
| `Task027N_2_9_DinoAttachedDirectionalBloodBurstClosure` | (see report) |
| `Task027N_2_BloodAudioBiteEndDebugGodMode` | (see report) |
| `Task027_WeaponExecutionEffectsFacesClosure` | Task027 — Complete Weapon Execution, Muzzle, Casing, Projectile/Effect + Player Face Consumer Closure |
