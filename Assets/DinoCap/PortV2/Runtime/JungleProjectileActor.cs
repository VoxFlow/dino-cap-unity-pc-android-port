using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed class JungleProjectileActor:MonoBehaviour
    {
        NativeJungleProjectile state;
        Power2DSourceTruthRenderer visual;
        Vector2 coordinateOffset;
        public NativeJungleProjectile State=>state;
        public void Initialize(DCManifest data,Vector2 dinosaurPosition,bool facingRight,Vector2 nativeCoordinateOffset=default)
        {
            coordinateOffset=nativeCoordinateOffset;
            state=new NativeJungleProjectile(dinosaurPosition+coordinateOffset,facingRight);
            visual=gameObject.AddComponent<Power2DSourceTruthRenderer>();
            visual.baseSortingOrder=216;visual.Initialize(data);
            //13918 writes renderer byte+96=1 independently of travel side.
            visual.facingRight=false;
            visual.primary.EnterFrame+=(track,frame)=>
            {
                if(track.Clip!=null && track.Clip.name=="dishangAnim")state.GroundFrame(frame);
            };
            transform.position=state.Position-coordinateOffset;
            visual.primary.Play(visual.FindClip("tuAnim"),0);
        }
        public void Tick(float dt,float playerX,bool invulnerable,Func<bool> hit)
        {
            if(state.Retired){Destroy(gameObject);return;}
            state.AdvancePhysics(dt);state.UpdateVelocity(dt);
            transform.position=state.Position-coordinateOffset;
            if(state.Flying)
            {
                //Native13A48 writes root angle;76A20 applies it only to
                //sprite angles. Rotating Unity's parent also rotated the
                //ground clip's -200..-232Y offsets, lifting goo into the air.
                visual.nativeRootRotationRadians=Mathf.Atan2(state.Velocity.y,state.Velocity.x);
                if(state.TryGround())
                {
                    transform.position=state.Position-coordinateOffset;
                    visual.primary.Play(visual.FindClip("dishangAnim"),0);
                }
                else if(state.InHitRange(playerX+coordinateOffset.x,invulnerable))
                {
                    //13D10 fatal branch returns before projectile teardown;
                    //only the nonfatal/Wudi branch retires it immediately.
                    if(hit()){state.Hit();Destroy(gameObject);}
                }
            }
        }
    }
}
