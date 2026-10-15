using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task027J3 source-faithful family-G special-fatal severed head.
    /// Native particleClassiPad kind 3:
    /// - one head, 40-slot source-style pool
    /// - facing-signed +/-100 X, +300 Y, gravity -2000
    /// - facing-signed angular velocity -/+4 rad/s
    /// - first floor contact bursts into four kind-2 chunks and removes head
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class FlyingHeadParticle : MonoBehaviour
    {
        public enum Species
        {
            K1Rusher = 1,
            K3Blue = 2,
            // TASK027N.1.3: Type 3 (k2). Native sub_1141C switches on the
            // dino species byte (dino+0xBC) - case 3 -> "k2toufeiFrame".
            // Landing debris uses the k2-specific atlas chunks (k2s1/k2s2),
            // not a reuse of K1/K3's.
            K2Trex = 3,
            K4Raptor = 4
        }

        const float Gravity = -2000f;

        SpriteRenderer sr;
        Power2DSourceTruthRenderer spriteSource;
        Species species;
        Vector2 velocity;
        float angularVelocity;
        float floorY;
        bool landed;

        public static FlyingHeadParticle Spawn(
            Power2DSourceTruthRenderer source,
            string refName,
            Vector3 position,
            Vector2 sourceSize,
            float floor,
            bool dinoFacingRight,
            Species sourceSpecies,
            int sortingOrder)
        {
            GameObject g=NativeDamageParticlePool.Acquire(source);
            g.name="Task027J3_FlyingHead_"+refName;
            g.transform.position=position;

            // Source contract: initial flying-head rotation is exactly 0.
            g.transform.rotation=Quaternion.identity;

            FlyingHeadParticle p=g.GetComponent<FlyingHeadParticle>() ?? g.AddComponent<FlyingHeadParticle>();
            p.enabled=true;p.landed=false;
            p.spriteSource=source;
            p.species=sourceSpecies;
            p.floorY=floor;
            p.velocity=new Vector2(
                dinoFacingRight ? +100f : -100f,
                +300f);
            p.angularVelocity=dinoFacingRight ? -4f : +4f;

            p.sr=g.GetComponent<SpriteRenderer>();
            p.sr.sortingOrder=sortingOrder;
            p.sr.flipX=dinoFacingRight;

            Sprite sp;
            if(sourceSpecies==Species.K4Raptor && refName==null)
            {
                // sub_1141C allocates/configures physics but skips the frame
                //switch for species4: preserve reclaimed sprite/size. A fresh
                //slot is empty, a reused slot may carry previous graphics.
            }
            else if(source==null || !source.TryGetSprite(refName,out sp))
            {
                p.sr.enabled=false;
                Debug.LogError("[DC1-V27] Missing flying-head ref "+refName);
            }
            else
            {
                p.sr.enabled=true;
                p.sr.sprite=sp;
                // Native sub_1141C writes gameSprite+44 directly from dino+0x60.
                // Unity SpriteRenderer.flipX is the corresponding mirror flag,
                // so preserve the same polarity (do NOT invert it).
                p.sr.flipX=dinoFacingRight;
                p.transform.localScale=new Vector3(
                    sourceSize.x/sp.rect.width,
                    sourceSize.y/sp.rect.height,
                    1f);
            }

            return p;
        }

        void Update()
        {
            // Pause freeze (native: the scene stops updating its objects in the paused state; the shared flag is Power2DSourceTruthRenderer.GlobalPaused).
            // Runs after the City controller's Update (execution order below) so the very frame the pause starts is already frozen.
            if(Power2DSourceTruthRenderer.GlobalPaused) return;
            if(landed)
                return;

            float dt=Time.deltaTime;
            velocity.y += Gravity*dt;

            transform.position += new Vector3(
                velocity.x*dt,
                velocity.y*dt,
                0f);

            transform.Rotate(
                0f,0f,
                angularVelocity*Mathf.Rad2Deg*dt);

            if(transform.position.y < floorY)
                Land();
        }

        void Land()
        {
            if(landed)
                return;
            landed=true;

            Vector3 p=transform.position;
            p.y=floorY;
            transform.position=p;

            SpawnLandingChunk(
                DinoCapDamageParticle.Kind.ChunkS3,
                new Vector2(61f,51f));
            SpawnLandingChunk(
                DinoCapDamageParticle.Kind.ChunkS4,
                new Vector2(50f,43f));

            if(species==Species.K1Rusher)
            {
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK4S1,
                    new Vector2(30f,49f));
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK4S2,
                    new Vector2(35f,33f));
            }
            else if(species==Species.K2Trex)
            {
                // TASK027N.1.3: exact raw-atlas rect for k2s1/k2s2 (PROVEN,
                // combat_manifest.json) - the same pass-through convention
                // K4's chunk sizes already use verbatim.
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK2S1,
                    new Vector2(44f,72f));
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK2S2,
                    new Vector2(52f,49f));
            }
            else if(species==Species.K4Raptor)
            {
                // DAA0 still allocates all four slots. Species 4 leaves slots
                //2/3 unassigned airborne; retain pool art, then xue6/xue7 on landing.
                SpawnLandingChunk(DinoCapDamageParticle.Kind.BlankChunkS1,Vector2.zero);
                SpawnLandingChunk(DinoCapDamageParticle.Kind.BlankChunkS2,Vector2.zero);
            }
            else
            {
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK3S1,
                    new Vector2(30f,49f));
                SpawnLandingChunk(
                    DinoCapDamageParticle.Kind.ChunkK3S2,
                    new Vector2(35f,33f));
            }

            if(sr!=null)
                sr.enabled=false;

            gameObject.SetActive(false);
        }

        void SpawnLandingChunk(
            DinoCapDamageParticle.Kind kind,
            Vector2 size)
        {
            // Native sub_DAA0 kind 3 uses the wider landing-burst velocity:
            // vx=(int)(rand*800)-400, vy=(int)(rand*200)+500.
            float vx=(int)(Random.value*800f)-400;
            float vy=(int)(Random.value*200f)+500;

            // DAA0 uses a positive magnitude {4,5}, independently of vx.
            float angular=(int)(Random.value*2f)+4f;

            DinoCapDamageParticle.Spawn(
                spriteSource,
                kind,
                transform.position,
                new Vector2(vx,vy),
                angular,
                floorY,
                size,
                sr!=null ? sr.sortingOrder : 11,
                Gravity);
        }
    }
}
