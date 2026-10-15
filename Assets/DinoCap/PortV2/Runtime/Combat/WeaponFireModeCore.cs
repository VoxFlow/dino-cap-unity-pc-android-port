namespace DinoCap.PortV2
{
    /// <summary>
    /// TASK027N.2.49R Phase 4 - shared, source-faithful fire-mode
    /// classification (native-proven: "C/D, F2 and F3 are native held-fire
    /// modes" - CombatSandboxV2.cs's own UpdateWeaponFireInput comment,
    /// itself citing prior native re-traces). A/B/E are semi-auto (one
    /// committed shot per press-edge); C/D/F2/F3 are held-full-auto
    /// (continuous fire for as long as the fire input is held).
    ///
    /// Previously this exact classification existed as inline duplicated
    /// boolean expressions in BOTH CombatSandboxV2.UpdateWeaponFireInput
    /// (its own `semiAutoMode`/held-mode checks) and CityGridController's
    /// own PollKeyboard (its own `cdHeldAutoFamily`/`cdHeldAutoFamilyLegacy`
    /// locals, added N.2.39 Part H for the mouse preset only). Factored out
    /// here so both consume the same decision, and so the keyboard preset
    /// (which N.2.39 never updated) can share it too.
    /// </summary>
    public static class WeaponFireModeCore
    {
        /// <param name="family">0=A, 1=B, 2=C, 3=D, 4=E, 5=F.</param>
        /// <param name="fTier">Only consulted when family==5: 0=F1(semi), 1=F2(held), 2=F3(held).</param>
        /// <returns>True for C/D/F2/F3 (held-full-auto); false for A/B/E/F1 (semi-auto, one shot per press-edge).</returns>
        public static bool IsHeldFullAutoFamily(int family, int fTier)
        {
            if(family==2 || family==3) return true;                 // C/D
            if(family==5 && (fTier==1 || fTier==2)) return true;     // F2/F3
            return false;                                            // A/B/E/F1
        }

        /// <summary>Overload for callers with no F-tier concept yet (City Grid currently only has A/B/C/D, never family 5) - equivalent to fTier=0 (never held for family 5).</summary>
        public static bool IsHeldFullAutoFamily(int family) => IsHeldFullAutoFamily(family, 0);
    }
}
