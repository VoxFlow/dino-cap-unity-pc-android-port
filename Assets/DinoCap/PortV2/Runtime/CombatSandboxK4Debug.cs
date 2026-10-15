#if true   // debug tools exist in every build (user decision 2026-10-13); the runtime DC1DebugSettings.Enabled switch is the only gate
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed partial class CombatSandboxV2
    {
        // N255: Sandbox already admits K4 through its dinosaur debug controls.
        // Reuse the proven Jungle projectile actor rather than inventing a
        // second attack. No automatic City/Jungle admission is changed.
        readonly List<JungleProjectileActor> debugK4Projectiles=new List<JungleProjectileActor>();
        void CreateDebugK4Projectile()
        {
            var go=new GameObject("SandboxDebugK4Projectile");
            go.transform.SetParent(transform,false);
            var actor=go.AddComponent<JungleProjectileActor>();
            // Sandbox actor ground roots are0; native City/Jungle roots136.
            // Translate physics coordinates only; retain native -60 ground
            // threshold and map the shared renderer back into Sandbox space.
            actor.Initialize(data,enemy.position,enemyFacingRight,Vector2.up*136f);
            debugK4Projectiles.Add(actor);
        }
        void TickDebugK4Projectiles(float dt)
        {
            for(int i=debugK4Projectiles.Count-1;i>=0;i--)
            {
                var actor=debugK4Projectiles[i];
                if(actor==null){debugK4Projectiles.RemoveAt(i);continue;}
                actor.Tick(dt,player.position.x,playerDead || Time.time<invulnUntil,DebugK4ProjectileHit);
            }
        }
        bool DebugK4ProjectileHit()
        {
            float damage=JungleBlitzProgression.Load().Query(0,4).attack;
            if(!godMode && playerHP<=damage)
            {
                DamagePlayer(damage);return false; // native fatal return keeps projectile
            }
            if(!godMode)playerHP-=damage;
            //13B10 grants Wudi/RoleAudio, without bite hurt, knockback,
            //facing change, or sustained weapon interruption.
            invulnUntil=Time.time+1.5f;
            //13B10 sets Wudi without restarting65C78 brightness/rate.
            NativeAudioSessionGate.Play(meleeAudioSource,playerHurtClip);
            return true;
        }
        internal int TestHarness_DebugK4ProjectileCount=>debugK4Projectiles.Count;
        internal void TestHarness_DebugK4Tick(float dt)=>TickDebugK4Projectiles(dt);
        internal bool TestHarness_DebugK4Hit()=>DebugK4ProjectileHit();
    }
}
#endif
