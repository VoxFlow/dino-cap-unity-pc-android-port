using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task026 compatibility particle for the currently playable K1 slice.
    /// No Destroy(timer) lifetime: settled decals remain until the 40-slot
    /// source-style pool reclaims the oldest slot.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class DinoCapDamageParticle : MonoBehaviour
    {
        public enum Kind
        {
            Blood = 0,
            ChunkS3 = 1,
            ChunkS4 = 2,
            ChunkK3S1 = 3,
            ChunkK3S2 = 4,
            ChunkK4S1 = 5,
            ChunkK4S2 = 6,
            // TASK027N.1.3: Type 3 (k2) decapitation landing debris. Sprite
            // refs "k2s1"/"k2s2" already exist in the raw atlas (added with
            // the rest of the k2 species set in Task027N.1) but were never
            // wired to a Kind until now.
            ChunkK2S1 = 7,
            ChunkK2S2 = 8,
            BlankChunkS1 = 9,
            BlankChunkS2 = 10
        }

        // TASK027N.2.7 CORRECTION: hand-decoded IEEE-754 re-verification of
        // native sub_173AF0's gravity argument (bit pattern 0xC4F00000, as
        // passed from sub_11124) proved this constant is EXACTLY -1920.0,
        // not -2144.0 as previously shipped. -2144 made every particle fall
        // measurably faster than native, shortening the perceivable
        // impact-local/rising phase of the ordinary-hit blood spurt.
        const float Gravity = -1920f;

        // TASK027N.2.2 Part B: DEBUG_ONLY spawn-count instrumentation - the
        // smallest addition needed to count actual particle-creation EVENTS
        // (not the `live` pool's steady-state size, which is capped/recycled
        // and therefore not hit-scoped). Counts every Spawn() call by Kind,
        // resettable so a test can zero it immediately before one hit and
        // read the exact delta immediately after. No gameplay behavior is
        // altered by this block - Spawn()'s own logic is unchanged below.
#if true   // spawn counters are used by the debug panel in every build
        static readonly Dictionary<Kind,int> spawnCallCounts = new Dictionary<Kind,int>();
        public static void TestHarness_ResetSpawnCounters() => spawnCallCounts.Clear();
        public static int TestHarness_SpawnCallCount(Kind k) =>
            spawnCallCounts.TryGetValue(k,out int n) ? n : 0;
#endif

        SpriteRenderer sr;
        Power2DSourceTruthRenderer spriteSource;
        Kind kind;
        Vector2 velocity;
        float angularVelocity;
        float floorY;
        float gravity=Gravity;
        bool settled;
        Sprite settledSprite;

        public static DinoCapDamageParticle Spawn(
            Power2DSourceTruthRenderer source,
            Kind particleKind,
            Vector3 position,
            Vector2 initialVelocity,
            float angularVelocityRadians,
            float floor,
            Vector2 airborneSize,
            int sortingOrder,
            float gravityAcceleration=Gravity)
        {
#if true   // spawn counters are used by the debug panel in every build
            spawnCallCounts.TryGetValue(particleKind,out int cur);
            spawnCallCounts[particleKind]=cur+1;
#endif
            GameObject g=NativeDamageParticlePool.Acquire(source);
            g.name="Task026_"+particleKind;
            g.transform.position=position;
            g.transform.rotation=Quaternion.identity;

            DinoCapDamageParticle p=g.GetComponent<DinoCapDamageParticle>() ?? g.AddComponent<DinoCapDamageParticle>();
            p.enabled=true;p.settled=false;p.settledSprite=null;
            p.spriteSource=source;
            p.kind=particleKind;
            if(source!=null) source.TryGetSprite(SettledRef(particleKind),out p.settledSprite);
            p.velocity=initialVelocity;
            p.angularVelocity=angularVelocityRadians;
            p.floorY=floor;
            p.gravity=gravityAcceleration;
            p.sr=g.GetComponent<SpriteRenderer>();
            p.sr.sortingOrder=sortingOrder;

            p.SetAirborneSprite(airborneSize);
            return p;
        }

        // TASK027N.2.49R Part C: shared-core extraction of the ordinary
        // (non-lethal-hit) ballistic blood droplet spawn, PROVEN native
        // sub_11124 (TASK027N.2.7's own IEEE-754-corrected velocity
        // constants: vy multiplier 200f/base+700, vx range [-400,400) via
        // 800f). Previously duplicated only inside CombatSandboxV2's own
        // private SpawnTask026Blood(); that method now delegates here
        // (TASK027N.2.49R) so City Grind can call the identical, unchanged
        // native-derived randomization rather than reimplementing it.
        // TASK027N.2.49R Phase 3: shared-core extraction of the standard-
        // death chunk spawn (native sub_11980, TASK027N.2.8's own IEEE-754-
        // corrected velocity constants: vx=[-400,400) via 800f, vy=[500,700)
        // via 200f+500 base, angular=+/-[3,6) opposing vx's own sign).
        // Previously duplicated only inside CombatSandboxV2's own private
        // SpawnTask026Chunk(); that method now delegates here so City Grind
        // can call the identical randomization.
        public static DinoCapDamageParticle SpawnDeathChunk(
            Power2DSourceTruthRenderer source,
            Kind chunkKind,
            Vector3 head,
            Vector2 airborneSize,
            float floorY,
            int sortingOrderBase)
        {
            // sub_11980 ordinary corpse debris differs from DAA0's
            // flying-head landing burst: native literal 0x43C80000 is 400.
            float vx = (int)(UnityEngine.Random.value * 400f) - 200;
            float vy = (int)(UnityEngine.Random.value * 200f) + 500;
            float mag = (int)(UnityEngine.Random.value * 3f) + 3f;
            float angular = vx >= 0f ? -mag : +mag;
            return Spawn(
                source, chunkKind, head,
                new Vector2(vx, vy), angular, floorY, airborneSize,
                sortingOrderBase + 60,-2000f);
        }

        public static DinoCapDamageParticle SpawnOrdinaryBlood(
            Power2DSourceTruthRenderer source,
            Vector3 head,
            float floorY,
            int sortingOrderBase)
        {
            float scalar = UnityEngine.Random.value + 1f;
            Vector2 size = new Vector2(28f * scalar, 13f * scalar);
            float vx = (int)(UnityEngine.Random.value * 800f) - 400;
            float vy = (int)(UnityEngine.Random.value * 200f) + 700;
            return Spawn(
                source,
                Kind.Blood,
                head,
                new Vector2(vx, vy),
                0f,
                floorY,
                size,
                sortingOrderBase + 60);
        }

        // Native player6833C: six xueFrame particles. Death uses28.5 and
        // -2000, unlike ordinary-hit11124's28 and-1920. Landing/pool reuse
        // stays on the shared particleClassiPad/DAA0 path.
        public static void SpawnPlayerDeathBlood(
            Power2DSourceTruthRenderer source, Vector3 head,
            float floorY, int sortingOrder)
        {
            for(int i=0;i<6;i++)
            {
                float scalar=UnityEngine.Random.value+1f;
                float vx=(int)(UnityEngine.Random.value*800f)-400;
                float vy=(int)(UnityEngine.Random.value*200f)+700;
                Spawn(source,Kind.Blood,head,new Vector2(vx,vy),0f,floorY,
                    new Vector2(28.5f*scalar,13f*scalar),sortingOrder,-2000f);
            }
        }

        void SetAirborneSprite(Vector2 size)
        {
            if(kind==Kind.BlankChunkS1 || kind==Kind.BlankChunkS2)
            {
                //Legacy enum names mean "no native assignment", not blank.
                //Retain the reclaimed slot's graphics and size (584B4).
                return;
            }
            string refName =
                kind==Kind.Blood ? "xue" :
                kind==Kind.ChunkS3 ? "s3" :
                kind==Kind.ChunkS4 ? "s4" :
                kind==Kind.ChunkK3S1 ? "k3s1" :
                kind==Kind.ChunkK3S2 ? "k3s2" :
                kind==Kind.ChunkK4S1 ? "k4s1" :
                kind==Kind.ChunkK4S2 ? "k4s2" :
                kind==Kind.ChunkK2S1 ? "k2s1" : "k2s2";

            Sprite sp;
            if(spriteSource==null || !spriteSource.TryGetSprite(refName,out sp))
            {
                sr.enabled=false;
                return;
            }

            sr.enabled=true;
            sr.sprite=sp;
            transform.localScale=new Vector3(
                size.x/sp.rect.width,
                size.y/sp.rect.height,
                1f);
        }

        // TASK027N.2.8 Part L: DEBUG_ONLY slow-motion aid so the user can
        // compare the first fraction of a second of blood-particle motion
        // against the original iPad frame-by-frame (numeric source closure
        // is proven, per this task's report, but full VISUAL acceptance of
        // the "impact-local spurt" question explicitly remains open and
        // requires this kind of side-by-side comparison). Defaults to 1f
        // (zero effect on production timing) and affects ONLY this class's
        // own per-frame integration - no other gameplay system reads it.
        internal static float DebugTimeScale = 1f;

        void Update()
        {
            // Pause freeze (native: the scene stops updating its objects in the paused state; the shared flag is Power2DSourceTruthRenderer.GlobalPaused).
            // Runs after the City controller's Update (execution order below) so the very frame the pause starts is already frozen.
            if(Power2DSourceTruthRenderer.GlobalPaused) return;
            if(settled)
                return;

            float dt=Time.deltaTime*DebugTimeScale;
            velocity.y += gravity*dt;
            transform.position +=
                new Vector3(velocity.x*dt,velocity.y*dt,0f);

            if(kind==Kind.Blood)
            {
                if(velocity.sqrMagnitude>0.0001f)
                {
                    float deg=Mathf.Atan2(velocity.y,velocity.x)*Mathf.Rad2Deg;
                    transform.rotation=Quaternion.Euler(0f,0f,deg);
                }
            }
            else
            {
                transform.Rotate(0f,0f,angularVelocity*Mathf.Rad2Deg*dt);
            }

            if(transform.position.y < floorY)
                Settle();
        }

        void Settle()
        {
            settled=true;
            velocity=Vector2.zero;
            angularVelocity=0f;

            Vector3 p=transform.position;
            // Native sub_DAA0: continuous [0,4), not integer {0..4}.
            p.y=floorY+Random.Range(0f,4f);
            transform.position=p;
            transform.rotation=Quaternion.identity;

            string refName;
            Vector2 size;

            if(kind==Kind.Blood)
            {
                refName="xue2";
                size=new Vector2(45f,4f);
            }
            else if(kind==Kind.ChunkS3)
            {
                refName="xue5";
                size=new Vector2(66.5f,17f);
            }
            else if(kind==Kind.ChunkS4)
            {
                refName="xue4";
                size=new Vector2(56.25f,15f);
            }
            else if(kind==Kind.ChunkK3S1 ||
                    kind==Kind.ChunkK4S1 ||
                    kind==Kind.ChunkK2S1 || kind==Kind.BlankChunkS1)
            {
                // Native sub_DAA0 case 2 settles by piece index, not species:
                // species s1 -> xue6.
                refName="xue6";
                size=new Vector2(65.5f,12f);
            }
            else
            {
                // species s2 -> xue7.
                refName="xue7";
                size=new Vector2(65.5f,13f);
            }

            Sprite sp=settledSprite;
            if(sp==null)
            {
                sr.enabled=false;
                return;
            }

            sr.sprite=sp;
            sr.enabled=true;
            transform.localScale=new Vector3(
                size.x/sp.rect.width,
                size.y/sp.rect.height,
                1f);
        }

        static string SettledRef(Kind k) => k==Kind.Blood ? "xue2" :
            k==Kind.ChunkS3 ? "xue5" : k==Kind.ChunkS4 ? "xue4" :
            (k==Kind.ChunkK3S1 || k==Kind.ChunkK4S1 || k==Kind.ChunkK2S1 || k==Kind.BlankChunkS1) ? "xue6" : "xue7";
    }
}
