using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// TASK027N.2.9 - source-faithful "Blood Impact Burst" (native sub_10ECC).
    /// Genuinely distinct from the ballistic xue/xue2 blood system
    /// (sub_11124/DinoCapDamageParticle, frozen, untouched by this class):
    /// a single sprite, one-shot non-looping transform pulse (grows then
    /// collapses, no alpha fade), mounted to the dino (Unity Transform
    /// parenting is the PLATFORM_ADAPTER equivalent of native's proven
    /// Triniti2D::MountManager::Mount call), with NO gravity, velocity,
    /// ground collision, or persistence.
    ///
    /// Two native variants, selected by the caller (native sub_10ECC's own
    /// "meleeFlag" argument):
    ///  - melee=true  -> "dxEffectAnimation" (7 frames, ref "daoxue") -
    ///    VISIBLE, red spiky sprite (PROVEN pixel-level, Task019 registry).
    ///  - melee=false -> "qxEffectAnimation" (5 frames, ref "qiangxue") -
    ///    N.2.9 shipped this invisible (genuine shipped quirk, per then-
    ///    available evidence). N.2.10/N.2.11 found a real "qiangxueAnim"
    ///    registry entry (index 761) native's loader ALSO builds, and
    ///    reasoned this made "truly invisible" unproven, so this class was
    ///    changed to render VISIBLE using daoxue's own real pixels as an
    ///    unproven PLATFORM_ADAPTER (add_qiangxue_burst_n210.py).
    ///    TASK027N.2.12 REVERSED THAT: decompiled the native a2b loader
    ///    (sub_78658/sub_780D8/sub_77A28) and found the Frame/Anim selector
    ///    byte is zero-initialized at PART-CONSTRUCTION time for every part
    ///    of every a2b clip. This was reported as unconditional/final proof
    ///    of "Frame path always, Anim path never" - GENUINELY INVISIBLE.
    ///    TASK027N.2.19 REVERSED THAT AGAIN, with a deeper instruction-level
    ///    trace (parent-session independently re-verified via raw
    ///    disassembly, not just decompiler text): N.2.12 only saw the
    ///    zero-init at construction time. A SEPARATE, later call -
    ///    sub_78D64(clipHandle,"com1") - runs unconditionally immediately
    ///    after BOTH qx.a2b AND dx.a2b finish loading (confirmed at raw
    ///    instruction addresses 0x3dbd0 and 0x3dc04 in sub_2A8A8), forcing
    ///    the SAME "com1" part's flag to 1 (Anim path) in every frame of
    ///    BOTH clips and re-resolving ref+"Anim". For daoxue, "daoxueAnim"
    ///    does not exist (PROVEN, N.2.10) so this re-resolution fails - daoxue
    ///    still renders correctly via a fallback not yet fully traced. For
    ///    qiangxue, "qiangxueAnim" DOES exist as a real, resolvable entry
    ///    (index 761, PROVEN N.2.10) - meaning qiangxue's lookup SUCCEEDS,
    ///    unlike daoxue's. Native almost certainly DOES play real content
    ///    for qiangxue (sub_174780 with a valid, non-(-1) index) - N.2.12's
    ///    "genuinely invisible" conclusion is superseded.
    ///    TASK027N.2.20 RESOLVED THE EXACT CONTENT: rather than finding the
    ///    concrete GetTextureAnimation() vtable override (not found within
    ///    budget - abstract Triniti2D::UIContainer::GetTextureAnimation()
    ///    stayed a dead end, see qx_reachability_reversal_n219.json), this
    ///    task traced sub_2A8A8's own asset-registration call
    ///    TextureManager::CreateAnimation("qiangxueAnim") (0x3b39a) forward
    ///    and found two concrete TextureManager::AddAnimationFrame(
    ///    "qiangxueAnim", frameKey) calls immediately following it
    ///    (0x3b438 frameKey="qiang0001Frame"; a second, structurally
    ///    identical call shortly after 0x3b4e8 frameKey="qiang0002Frame") -
    ///    PROVEN at the raw-instruction level. Both frame keys resolve to
    ///    real, already-catalogued Task019 registry entries on the SAME
    ///    shared iPad dino2.png atlas daoxue itself lives on: qiang0001Frame
    ///    (x=600,y=549,w=176,h=137) and qiang0002Frame (x=848,y=478,w=176,
    ///    h=137). N.2.10 had dismissed these exact two strings as "likely an
    ///    unrelated GUN/WEAPON icon" based on dimension mismatch with daoxue
    ///    alone (135x144) - that dismissal is FALSIFIED: direct pixel crop
    ///    this task shows both are dark-red jagged spike-fan burst
    ///    silhouettes, average opaque-pixel RGB (152.7,0,0) - color-identical
    ///    to daoxue's own (152.75,0,0) - i.e. genuinely distinct blood-burst
    ///    art from the same style/color family as daoxue, not a weapon icon,
    ///    not an exact daoxue pixel reuse. Classification:
    ///    B. DISTINCT_QIANGXUE_ART (see qiangxue761_vs_daoxue_n220.json).
    ///    SOURCE_QX_EFFECT: PROVEN VISIBLE, real distinct native content
    ///    (qiang0001Frame/qiang0002Frame, index 761, 2-frame flipbook).
    ///    PORT_VISIBLE_QX_ADAPTER: RETIRED - the atlas rects below now point
    ///    at native's own real qiangxue pixels (fix_qiangxue_pixels_n220.py)
    ///    instead of a borrowed daoxue placeholder; classification upgraded
    ///    to SOURCE_FAITHFUL for the sprite identity. qxEffectAnimation's own
    ///    distinctly-authored transform-pulse timing/geometry (5 frames, its
    ///    own W/H/X/Y curve, non-identical to dx's 7-frame curve per N.2.10)
    ///    remains untouched - only the terminal sprite-pixel identity
    ///    changed. Playback rate/exact frame order for the underlying native
    ///    2-frame TextureManager animation itself was not independently
    ///    traced (out of this task's bounded scope once the frame content
    ///    was proven) and remains UNRESOLVED/LIKELY only in that narrow
    ///    respect; it does not affect this adapter, which continues to use
    ///    qxEffectAnimation's own already-proven native clip timing.
    ///    See qiangxue761_exact_frames_n220.json /
    ///    qiangxue761_vs_daoxue_n220.json for full detail.
    /// </summary>
    public static class DinoCapBloodBurst
    {
        // Native sub_10ECC general-case offset: lerp(C*half, half, randNorm)
        // where randNorm = (float)rand() * 0x30000000 (~rand()/2^31), hand-
        // decoded from raw ARM this task and independently re-verified via
        // Python (exact bit-pattern match). C = -0.3 (X), -0.8 (Y). Random.
        // value substitutes for the scaled rand() call (both approximate a
        // [0,1) uniform draw).
        const float GeneralOffsetXLo = -0.3f, GeneralOffsetXHi = 1.0f;
        const float GeneralOffsetYLo = -0.8f, GeneralOffsetYHi = 1.0f;

        // Native sub_10ECC Type3-species-melee-only override: FIXED (non-
        // random) offset - 0.8*halfW, -0.1*halfH - plus a facing-based (not
        // random) flip. Hand-decoded from raw ARM this task.
        const float Type3OffsetXFactor = 0.8f, Type3OffsetYFactor = -0.1f;

        // TASK027N.2.21 Part O: DEBUG_ONLY observability, not read by
        // gameplay. Counts every Spawn() call since the last ResetDebugCounts
        // - lets the debug overlay show how many qx/dx burst objects were
        // created per damage event, to visually confirm the N.2.21 fix for
        // the qiangxue-duplication bug (the old redundant
        // WeaponEffectAnimation-pooled spawn in SpawnLightImpact/
        // CombatSandboxV2 is what used to make this count 2 instead of 1).
        public static int QxSpawnCount { get; private set; }
        public static int DxSpawnCount { get; private set; }
        public static void ResetDebugCounts() { QxSpawnCount = 0; DxSpawnCount = 0; }

        // TASK027N.2.51 THIRD REOPEN Family A (item 12 - player death lacks
        // ballistic blood): headPartName ADDED, default "tou" (dinos' own
        // rig part name, UNCHANGED for every existing dino call site).
        // PROVEN root cause via direct empirical test (N251_36): this
        // function's own TryGetPartHalfExtents("tou",...)/TryGetPartWorldAnchor
        // both look up a part by name WITHIN THE CURRENTLY-PLAYING CLIP's
        // own frame data (Power2DSourceTruthRenderer.primary.Current), not a
        // manifest-wide part list - and the player's own rig does not carry
        // a part named "tou" (dinos and the player are separately-authored
        // rigs). The player's own head anchor is already independently
        // PROVEN elsewhere in this project as "yyj6" (player_death_contract.json's
        // own sub_6833C archaeology: native's blood-spawn loop uses the
        // player's "yyj6" part, cross-referenced as 'si tou'/head in the
        // p_die manifest data) - passed explicitly from the player's own
        // call site below, not guessed.
        public static void Spawn(
            Power2DSourceTruthRenderer owner,
            DCManifest data,
            bool melee,
            bool type3FacingOverride,
            int sortingOrder,
            string headPartName = "tou")
        {
            if (owner == null || data == null)
                return;

            float halfW, halfH;
            if (!owner.TryGetPartHalfExtents(headPartName, out halfW, out halfH))
                return;

            float offsetX, offsetY;
            bool flip;

            if (type3FacingOverride && melee)
            {
                offsetX = halfW * Type3OffsetXFactor;
                offsetY = halfH * Type3OffsetYFactor;
                flip = owner.facingRight;
            }
            else
            {
                offsetX = halfW * (GeneralOffsetXLo +
                    (GeneralOffsetXHi - GeneralOffsetXLo) * UnityEngine.Random.value);
                offsetY = halfH * (GeneralOffsetYLo +
                    (GeneralOffsetYHi - GeneralOffsetYLo) * UnityEngine.Random.value);
                flip = offsetX < 0f;
            }

            Vector3 anchor;
            if (!owner.TryGetPartWorldAnchor(headPartName, out anchor))
                anchor = owner.transform.position;

            if (melee) DxSpawnCount++; else QxSpawnCount++;

            GameObject g = new GameObject(
                melee ? "Task027N29_BloodBurst_dx" : "Task027N29_BloodBurst_qx");
            g.transform.SetParent(owner.transform, true);
            CityGridController.TrackActorSceneEffect(g.transform, owner);
            g.transform.position = anchor + new Vector3(offsetX, offsetY, 0f);
            g.transform.rotation = Quaternion.identity;

            Power2DSourceTruthRenderer r =
                g.AddComponent<Power2DSourceTruthRenderer>();
            r.actorScale = owner.actorScale;
            r.baseSortingOrder = sortingOrder;
            r.facingRight = flip;
            r.Initialize(data);

            DCAnimation clip = r.FindClip(
                melee ? "dxEffectAnimation" : "qxEffectAnimation");
            r.primary.Play(clip, 0);

            float duration = (clip.frames != null && clip.frames.Length > 0)
                ? clip.frames.Length * clip.frameDuration
                : 0.175f;
            AttachLifetime(g, duration);
        }

        // Pause freeze (PLATFORM_ADAPTER for the proven native pause = scene time scale 0). The native removal path of this effect object was NOT
        // traced (OPEN); the lifetime keeps the pre-existing nominal duration (frames x frameDuration) and is not bound to clip completion.
        // Object.Destroy(g, delay) keeps counting during a pause, so the lifetime is counted here instead and only advances while unpaused.
        internal static void AttachLifetime(GameObject g, float seconds)
        {
            DinoCapBloodBurstLifetime l = g.AddComponent<DinoCapBloodBurstLifetime>();
            l.remaining = seconds;
        }
    }

    [DefaultExecutionOrder(100)]
    sealed class DinoCapBloodBurstLifetime : MonoBehaviour
    {
        internal float remaining;

        void Update()
        {
            if(Power2DSourceTruthRenderer.GlobalPaused) return;
            remaining -= Time.deltaTime;
            if(remaining <= 0f) Destroy(gameObject);
        }
    }
}
