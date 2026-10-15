namespace DinoCap.PortV2
{
    /// <summary>
    /// TASK027N.2.49R Phase 3 remainder - shared, source-faithful fatal-death
    /// PRESENTATION-SELECTION core (native sub_10B90 decapitation-eligibility
    /// gate + sub_1141C species-head dispatch, PROVEN via CombatSandboxV2's
    /// own extensive citation trail - see BeginGSpecialFatalDeath/
    /// BeginEWeaponDeath there, unchanged, only its DECISION SHAPE and
    /// per-species/per-mode DATA are factored out here).
    ///
    /// This is the fatal-branch counterpart to DinoHeavyHitCore (which
    /// explicitly scoped OUT "the fatal/death branch" as a known follow-up -
    /// see that file's own header comment). Closing that follow-up is this
    /// class's whole purpose.
    ///
    /// Scope: ONLY the mode-selection and per-species/per-mode DATA (clip
    /// names, particle ref names, sizes). Deliberately does NOT perform any
    /// particle-spawn/animation-play/audio-play itself and does NOT define
    /// an adapter interface over the caller's own dino/visual type (per the
    /// same "do not over-engineer" precedent DinoHeavyHitCore already
    /// established) - each caller (CombatSandboxV2, CityGridController)
    /// applies the returned data using its own already-established
    /// particle/visual API calls.
    /// </summary>
    public static class DinoFatalDeathCore
    {
        /// <summary>Native: Standard (default/G1-G2), EExplosion (E family, AoE blast), GSpecial (F1 or G3/G4 - decapitation).</summary>
        public enum HeavyFatalMode { Standard, EExplosion, GSpecial }

        /// <summary>
        /// PROVEN (CombatSandboxV2.ExecuteMeleeContact): meleeTier 0/1 (G1/G2)
        /// -> Standard; meleeTier 2/3 (G3/G4) -> GSpecial.
        /// F1 (family 5, fMode 0) is unconditionally GSpecial with
        /// spawnBlood:false - no helper needed, a caller-side literal since
        /// there is no tier/species input to branch on.
        /// E (family 4) is unconditionally EExplosion - likewise a literal.
        /// </summary>
        public static HeavyFatalMode MeleeFatalMode(int meleeTier) =>
            UnityEngine.Mathf.Clamp(meleeTier, 0, 3) >= 2 ? HeavyFatalMode.GSpecial : HeavyFatalMode.Standard;

        /// <summary>
        /// Species identity for fatal-death presentation purposes. Matches
        /// CombatSandboxV2's own DebugDinoKind ordering/naming exactly (both
        /// are the same underlying native dino+0xBC species byte). City
        /// Grind's own CityDinoKind{Type1,Type2,Type3} maps directly:
        /// Type1=K3Blue, Type2=K1Rusher, Type3=K2Trex - City Grind has no
        /// K4Raptor-equivalent species in its own roster at all.
        /// </summary>
        public enum Species { K3Blue, K1Rusher, K2Trex, K4Raptor }

        public struct GSpecialPresentation
        {
            /// <summary>
            /// Whether the special-death owner is available. Native sub_1141C
            /// continues into DieAnim/death2 even when species 4 has no head
            /// frame assignment; a missing head asset does not select ordinary death.
            /// </summary>
            public bool supported;
            public string die2ClipName;
            public string flyingHeadRefName;
            public float flyingHeadWidth, flyingHeadHeight;
            public FlyingHeadParticle.Species particleSpecies;
        }

        /// <summary>
        /// PROVEN (CombatSandboxV2.BeginGSpecialFatalDeath, native sub_1141C
        /// species switch): case 1(K3Blue, this enum's default/else branch)
        /// -&gt; k3toufeiFrame; case 2(K1Rusher) -&gt; k4toufeiFrame (a
        /// PRESERVED shipped raw-asset-prefix/semantic-species mismatch,
        /// Task027J4 quirk - do not "fix" this); case 3(K2Trex) -&gt;
        /// k2toufeiFrame; no case 4(K4Raptor).
        /// </summary>
        public static GSpecialPresentation GetGSpecialPresentation(Species species)
        {
            switch (species)
            {
                case Species.K4Raptor:
                    return new GSpecialPresentation
                    {
                        supported = true,
                        die2ClipName = "k4njz_die2",
                        flyingHeadRefName = null,
                        particleSpecies = FlyingHeadParticle.Species.K4Raptor
                    };
                case Species.K1Rusher:
                    return new GSpecialPresentation
                    {
                        supported = true,
                        die2ClipName = "k4_die2",
                        flyingHeadRefName = "k4toufei",
                        flyingHeadWidth = 116f,
                        flyingHeadHeight = 133f,
                        particleSpecies = FlyingHeadParticle.Species.K1Rusher
                    };
                case Species.K2Trex:
                    return new GSpecialPresentation
                    {
                        supported = true,
                        die2ClipName = "k2_die2",
                        flyingHeadRefName = "k2toufei",
                        flyingHeadWidth = 170f,
                        flyingHeadHeight = 164f,
                        particleSpecies = FlyingHeadParticle.Species.K2Trex
                    };
                default: // K3Blue
                    return new GSpecialPresentation
                    {
                        supported = true,
                        die2ClipName = "k3_die2",
                        flyingHeadRefName = "k3toufei",
                        flyingHeadWidth = 116f,
                        flyingHeadHeight = 120f,
                        particleSpecies = FlyingHeadParticle.Species.K3Blue
                    };
            }
        }

        public struct EWeaponSuirouSpec { public string refName; public float width, height; }

        /// <summary>
        /// PROVEN (CombatSandboxV2.BeginEWeaponDeath): exactly 4 fixed
        /// suirou gib parts, species-independent, anchored at the "shen"
        /// (body) socket.
        /// </summary>
        public static readonly EWeaponSuirouSpec[] EWeaponSuirouParts = new EWeaponSuirouSpec[]
        {
            new EWeaponSuirouSpec { refName = "suirou2", width = 35f,   height = 19f },
            new EWeaponSuirouSpec { refName = "suirou3", width = 38.5f, height = 23f },
            new EWeaponSuirouSpec { refName = "suirou4", width = 32f,   height = 32.5f },
            new EWeaponSuirouSpec { refName = "suirou5", width = 36f,   height = 35f },
        };

        /// <summary>PROVEN (CombatSandboxV2.BeginEWeaponDeath): exactly 3 ordinary blood spawns follow the 4 suirou parts.</summary>
        public const int EWeaponBloodSpawnCount = 3;

        /// <summary>
        /// TASK027N.2.49R Phase 5 remainder (shared Ash-Death branch) -
        /// PROVEN native sub_118E8, called EXACT-shared by F2 (type-5
        /// tier-1) and F3 (tier-2), with species-specific scale/position writes
        /// (see shared_ash_death_contract_n249r.json for the full
        /// evidence). This is genuinely shared presentation, not an
        /// assumed coincidence merely because both end in ash/bones -
        /// verified against the source's own explicit citation before
        /// being factored out here. F2/F3-specific bits (death audio clip,
        /// reward, burn/reaction state-flag bookkeeping) correctly stay in
        /// each caller, not here.
        /// </summary>
        public static void PlaySharedAshDeath(Power2DSourceTruthRenderer visual,Species species=Species.K3Blue)
        {
            visual.secondary.Stop();
            // Native small species: living root = groundBaseline+240,
            // ShaoSi (ash-death) root = groundBaseline+192 => relative to
            // the living root: -48 source units.
            //118E8: species4 skips these writes; species3 only writes scale1.
            if(species==Species.K3Blue || species==Species.K1Rusher)
            {
                visual.actorScale = 0.8f;
                visual.drawOffset = new UnityEngine.Vector2(0f, -48f);
            }
            else if(species==Species.K2Trex)visual.actorScale=1f;
            visual.primary.Play(visual.FindClip("ShaoSiAnim"), 0);
        }
    }
}
