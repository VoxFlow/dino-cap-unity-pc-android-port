using System;

namespace DinoCap.PortV2
{
    // lifeModeToolsiPad:1AD48 spawn,1B0D0 bob,1B13C collection,
    // 1BB3C firsttimer/secondtimer expiration. Rendering and inventory
    // application remain separate owners; no invented sine-wave bob.
    public sealed class JunglePickupState
    {
        public enum Kind { Health=-1, Ammo=0, Weapon=1 }
        public Kind PickupKind { get; }
        public float X { get; }
        public float SpawnY { get; }
        public float BobPhase { get; private set; }=1f;
        public float VelocityY { get; private set; }
        public bool Active { get; private set; }=true;
        public bool Uncollected { get; private set; }=true;
        public bool HealIndicator => Active && !Uncollected && PickupKind==Kind.Health;
        float remaining=10f;
        readonly bool externalTimer;
        public JunglePickupState(Kind kind,float x,float deathY,bool externalTimer=false)
        {
            PickupKind=kind;X=x;SpawnY=deathY+20f;this.externalTimer=externalTimer;
        }
        public bool InPickupRange(float playerX) => Active && Uncollected && Math.Abs(playerX-X)<80f;
        public void Tick(float dt)
        {
            if(float.IsNaN(dt) || float.IsInfinity(dt) || dt<0f)throw new ArgumentOutOfRangeException(nameof(dt));
            if(!Active)return;
            if(!externalTimer){remaining-=dt;if(remaining<=0f) {Active=false;return;}}
            if(!Uncollected)return;
            BobPhase+=dt;
            if(BobPhase>1f)BobPhase=0f;
            else VelocityY=BobPhase<=.5f ? 20f : -20f;
            // Native >1 branch discards overshoot and leaves velocity
            // unchanged for that tick, rather than wrapping/modulo or sine.
        }
        public void Expire()=>Active=false;
        public bool TryCollectHealth(float playerX,ref float hp)
        {
            if(PickupKind!=Kind.Health || !InPickupRange(playerX))return false;
            hp=Math.Min(100f,hp+25f);
            Uncollected=false;remaining=5f;VelocityY=100f;
            return true;
        }
        public bool FinishInventoryCollection(float playerX)
        {
            if(PickupKind==Kind.Health || !InPickupRange(playerX))return false;
            Uncollected=false;Active=false;
            return true;
        }
    }
}
