using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task027 exact bulletClassiPad casing contract.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class WeaponCasing : MonoBehaviour
    {
        const int Capacity=20;
        const float Gravity=-2144f;

        static readonly Queue<WeaponCasing> live =
            new Queue<WeaponCasing>();

        SpriteRenderer sr;
        Vector2 velocity;
        float angularVelocity;
        float floorY;
        bool settled;

        public static void Spawn(
            Power2DSourceTruthRenderer source,
            string refName,
            Vector3 pos,
            bool playerFacingRight,
            float floor,
            int sortingOrder)
        {
            while(live.Count>=Capacity)
            {
                WeaponCasing old=live.Dequeue();
                if(old!=null)
                    Destroy(old.gameObject);
            }

            Sprite sp;
            if(source==null || !source.TryGetSprite(refName,out sp))
                return;

            GameObject g=new GameObject("Task027_Casing_"+refName);
            CityGridController.OwnSceneEffect(g.transform, source);
            g.transform.position=pos;

            WeaponCasing c=g.AddComponent<WeaponCasing>();
            c.sr=g.AddComponent<SpriteRenderer>();
            c.sr.sprite=sp;
            c.sr.sortingOrder=sortingOrder;
            c.transform.localScale=new Vector3(
                18.75f/sp.rect.width,
                8.875f/sp.rect.height,
                1f);

            float mag=(int)(UnityEngine.Random.value*51f)+150f;
            c.velocity=new Vector2(
                playerFacingRight ? -mag : mag,
                406f);
            c.angularVelocity=UnityEngine.Random.value*4f+10f;
            c.floorY=floor;
            live.Enqueue(c);
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
                velocity.x*dt,velocity.y*dt,0f);
            transform.Rotate(
                0f,0f,angularVelocity*Mathf.Rad2Deg*dt);

            if(transform.position.y<floorY)
                Bounce();
        }

        void Bounce()
        {
            Vector3 p=transform.position;
            p.y=2f*floorY-p.y;
            transform.position=p;

            velocity.y*=-0.5f;
            angularVelocity+=2f;

            if(Mathf.Abs(velocity.y)<50f)
            {
                velocity=Vector2.zero;
                angularVelocity=0f;
                p=transform.position;
                p.y=floorY;
                transform.position=p;
                transform.rotation=Quaternion.identity;
                settled=true;
            }
        }
    }
}
