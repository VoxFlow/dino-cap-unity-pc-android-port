using UnityEngine;

namespace DinoCap.PortV2
{
    //13760 constructor,13918 launch,13A48 physics,13B10 collision,
    //139C8 ground transition and13D64 marked ground frame18.
    public sealed class NativeJungleProjectile
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Phase { get; private set; }
        public bool Flying { get; private set; }=true;
        public bool Retired { get; private set; }
        readonly bool left;
        Vector2 appliedPhysicsVelocity;
        public NativeJungleProjectile(Vector2 dinosaurPosition,bool facingRight)
        {
            left=!facingRight;
            Position=dinosaurPosition+new Vector2(facingRight?25f:-25f,-145f);
            //Constructor C3960000=-300; rightward override only starts
            //inside13A48. Native13918 does not initialize velocity again.
            Velocity=new Vector2(-300f,0);
        }
        public void AdvancePhysics(float dt)
        {
            if(!Retired && Flying)Position+=appliedPhysicsVelocity*dt;
        }
        public void UpdateVelocity(float dt)
        {
            if(Retired || !Flying)return;
            Phase+=dt;
            float gravity=Phase>.1f?-100f:-50f;
            Velocity=new Vector2(left?Velocity.x:280f,Velocity.y+gravity*dt);
            appliedPhysicsVelocity=Velocity;
        }
        public bool InHitRange(float playerX,bool invulnerable)
            =>!Retired && Flying && Position.y>-60f &&
                Mathf.Abs(playerX-Position.x)<20f && !invulnerable;
        public bool TryGround()
        {
            if(Retired || !Flying || Position.y>-60f)return false;
            Flying=false;Velocity=Vector2.zero;Position+=Vector2.up*180f;return true;
        }
        public void Hit()=>Retired=true;
        public void GroundFrame(int frame){if(!Flying && frame==18)Retired=true;}
    }
}
