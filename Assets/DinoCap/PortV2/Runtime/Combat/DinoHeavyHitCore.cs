namespace DinoCap.PortV2
{
    /// <summary>
    /// TASK027N.2.49R Phase 2 - shared, source-faithful non-fatal heavy-hit
    /// reaction decision core (native sub_10554 case4/case6, PROVEN across
    /// prior Task027N tasks - see CombatSandboxV2.ApplyHeavyWeaponHit's own
    /// extensive citation trail, unchanged/uncopied here, only its DECISION
    /// SHAPE is factored out).
    ///
    /// Scope: ONLY the non-fatal reaction dispatch (Type3 permanent
    /// no-knockback exclusion vs. bite-committed damage-only vs. ordinary
    /// full heavy stagger). Deliberately does NOT include:
    ///  - the fatal/death branch (Standard/EExplosion/GSpecial) - callers
    ///    check hp&lt;=0 themselves and invoke their own death dispatch,
    ///    since EExplosion/GSpecial are topologically UNREACHABLE from City
    ///    Grind today (no E weapon, no G3/G4 tier exist there yet - see
    ///    shared_combat_core_n249r.json for the proof this is a genuine
    ///    dependency-ordering fact, not scope reduction);
    ///  - blood spawning - callers already call the separately-shared
    ///    DinoCapDamageParticle.SpawnOrdinaryBlood before this, matching
    ///    native's own "blood before branch" ordering;
    ///  - F1's own separate light-family bite gate (F1 does not exist in
    ///    City Grind yet either).
    ///
    /// No interface/abstraction over the caller's own dino type was
    /// introduced (per this task's own "do not over-engineer" instruction)
    /// - this is a plain static decision function over simple parameters;
    /// each caller applies the returned velocity/clip-suffix to its own
    /// instance fields in its own way.
    /// </summary>
    public static class DinoHeavyHitCore
    {
        public enum ReactionKind
        {
            /// <summary>Type3 species-permanent exclusion (native dino+0xBC==3) - always the light, non-knockback reaction regardless of bite state.</summary>
            Type3Light,
            /// <summary>Non-Type3, currently bite-committed (F1's own shared GSpecial path only - see caller note) - damage-only, no stagger.</summary>
            BiteCommittedDamageOnly,
            /// <summary>Non-Type3, not bite-committed - full heavy stagger: BehitedAnim + facing-relative +/-192 knockback velocity.</summary>
            HeavyStagger
        }

        public struct Result
        {
            public ReactionKind kind;
            /// <summary>Velocity to apply (0 unless kind==HeavyStagger).</summary>
            public float velocityX;
            /// <summary>True only for kind==HeavyStagger - caller should also arm its own "heavy reaction in progress" state so its own per-tick integrates velocityX until the reaction clip finishes.</summary>
            public bool armHeavyReactionState;
        }

        /// <param name="isType3">The target's own permanent species-Type3 flag (native dino+0xBC==3).</param>
        /// <param name="biteCommittedAndSpawnBloodFalse">Only ever true for F1's own shared-GSpecial call path (spawnBlood==false AND the target is currently bite-committed) - City Grind cannot produce this combination yet since it has no F1 weapon, so City Grind callers should always pass false here until Phase 5.</param>
        /// <param name="facingRight">The target's own current facing (used to pick the knockback direction sign).</param>
        public static Result ApplyNonFatalHeavyReaction(bool isType3, bool biteCommittedAndSpawnBloodFalse, bool facingRight)
        {
            Result r = new Result();
            if (isType3)
            {
                r.kind = ReactionKind.Type3Light;
                return r;
            }
            if (biteCommittedAndSpawnBloodFalse)
            {
                r.kind = ReactionKind.BiteCommittedDamageOnly;
                return r;
            }
            r.kind = ReactionKind.HeavyStagger;
            r.velocityX = facingRight ? -192f : +192f;
            r.armHeavyReactionState = true;
            return r;
        }
    }
}
