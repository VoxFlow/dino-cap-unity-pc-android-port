using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task027E exact E/type-4 airborne suirou chunk.
    /// Native kind4 settles to suirou1 and persists with the scene/pool.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class WeaponSuirouParticle : MonoBehaviour
    {
        const float Gravity=-2000f;


        SpriteRenderer sr;
        Sprite settledSprite;
        Vector2 velocity;
        float angularVelocity;
        float floorY;
        bool settled;

        public static void Spawn(
            Power2DSourceTruthRenderer source,
            string refName,
            Vector3 position,
            Vector2 baseSize,
            float floor,
            int sortingOrder)
        {

            Sprite sp;
            if(source==null || !source.TryGetSprite(refName,out sp))
                return;

            GameObject g=NativeDamageParticlePool.Acquire(source);
            g.name="Task027E_"+refName;
            g.transform.position=position;
            g.transform.rotation=Quaternion.identity;

            WeaponSuirouParticle p=g.GetComponent<WeaponSuirouParticle>() ?? g.AddComponent<WeaponSuirouParticle>();
            p.enabled=true;p.settled=false;p.settledSprite=null;
            //Native texture ownership is independent of the dying actor.
            //Retain the exact landing sprite before City retires that renderer.
            source.TryGetSprite("suirou1",out p.settledSprite);
            p.sr=g.GetComponent<SpriteRenderer>();
            p.sr.enabled=true;
            p.sr.sprite=sp;
            p.sr.sortingOrder=sortingOrder;

            float scale=1f+UnityEngine.Random.value;
            Vector2 size=baseSize*scale;
            p.transform.localScale=new Vector3(
                size.x/sp.rect.width,
                size.y/sp.rect.height,
                1f);

            float vx=(int)(UnityEngine.Random.value*800f)-400f;
            float vy=(int)(UnityEngine.Random.value*200f)+700f;
            float av=(int)(UnityEngine.Random.value*3f)+3f;
            if(vx>0f)
                av=-av;

            p.velocity=new Vector2(vx,vy);
            p.angularVelocity=av;
            p.floorY=floor;
        }

        void Update()
        {
            // Pause freeze (native: the scene stops updating its objects in the paused state; the shared flag is Power2DSourceTruthRenderer.GlobalPaused).
            // Runs after the City controller's Update (execution order below) so the very frame the pause starts is already frozen.
            if(Power2DSourceTruthRenderer.GlobalPaused) return;
            if(settled)
                return;

            float dt=Time.deltaTime;
            velocity.y+=Gravity*dt;
            transform.position+=new Vector3(
                velocity.x*dt,
                velocity.y*dt,
                0f);
            transform.Rotate(
                0f,0f,
                angularVelocity*Mathf.Rad2Deg*dt);

            if(transform.position.y<floorY)
                Settle();
        }

        void Settle()
        {
            // Task027F / native sub_DAA0 case 4:
            // zero motion/gravity callback state, swap every E airborne
            // suirou piece to the single common suirou1Frame decal, force
            // 123 x 28.375 source units, rotation 0, and place at
            // floorY + continuous Random[0,4). The object remains rendered
            // until the 40-slot pool recycles it.
            velocity=Vector2.zero;
            angularVelocity=0f;

            if(settledSprite==null)
            {
                // Missing exported source ref is a representation failure;
                // do not substitute another gore sprite.
                gameObject.SetActive(false);
                return;
            }

            sr.sprite=settledSprite;
            transform.localScale=new Vector3(
                123f/settledSprite.rect.width,
                28.375f/settledSprite.rect.height,
                1f);
            transform.rotation=Quaternion.identity;

            Vector3 p=transform.position;
            p.y=floorY+UnityEngine.Random.Range(0f,4f);
            transform.position=p;

            settled=true;
        }
    }
}
