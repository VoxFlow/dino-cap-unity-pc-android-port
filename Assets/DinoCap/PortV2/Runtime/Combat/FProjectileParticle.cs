using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// TASK027N.2.49R Phase 5 remainder - generalizes FProjectileParticle's
    /// own owner coupling from a concrete CombatSandboxV2 reference to this
    /// interface, so CityGridController can also own/receive F1 projectiles
    /// without a scene-local copy of the pooled-projectile class itself
    /// (matches the DinoHeavyHitCore/DinoFatalDeathCore "shared owner, per-
    /// scene application" precedent already established this task).
    /// </summary>
    public interface IF1ProjectileOwner
    {
        void TryF1ProjectileContact(FProjectileParticle projectile);
    }

    [DefaultExecutionOrder(100)]
    public sealed class FProjectileParticle : MonoBehaviour
    {
        const int Capacity=10;
        const float Speed=500f;

        static readonly List<FProjectileParticle> pool=
            new List<FProjectileParticle>();

        // Native13250 stores the LAST projectile on each victim (+180),
        // rather than a permanent per-flight set. Native13420 clears every
        // victim marker in this scene when any projectile leaves the view.
        static readonly Dictionary<Transform,FProjectileParticle> lastHits=
            new Dictionary<Transform,FProjectileParticle>();

        Power2DSourceTruthRenderer visual;
        IF1ProjectileOwner owner;
        bool activeProjectile;
        float velocityX;

        public static FProjectileParticle Spawn(
            DCManifest manifest,
            IF1ProjectileOwner owner,
            Vector3 position,
            bool facingRight,
            int sortingOrder)
        {
            pool.RemoveAll(item => item==null);
            FProjectileParticle p=null;
            for(int i=0;i<pool.Count;i++)
            {
                if(pool[i]!=null && !pool[i].gameObject.activeSelf)
                {
                    p=pool[i];
                    break;
                }
            }

            if(p==null)
            {
                if(pool.Count>=Capacity)
                    return null;

                GameObject g=new GameObject("Task027K_F1_Jupan");
                p=g.AddComponent<FProjectileParticle>();
                p.visual=g.AddComponent<Power2DSourceTruthRenderer>();
                p.visual.Initialize(manifest);
                pool.Add(p);
            }

            CityGridController.OwnSceneEffect(p.transform, owner as Component);
            p.owner=owner;
            p.activeProjectile=true;
            p.velocityX=facingRight ? +Speed : -Speed;

            p.gameObject.SetActive(true);
            p.transform.position=position;
            p.transform.rotation=Quaternion.identity;

            p.visual.facingRight=facingRight;
            p.visual.actorScale=1f;
            p.visual.baseSortingOrder=sortingOrder;
            p.visual.secondary.Stop();
            p.visual.primary.Play(
                p.visual.FindClip("jupanAnimation"),0);

            return p;
        }

        public bool HasHit(Transform target)
        {
            return target!=null && lastHits.TryGetValue(target,out var last) && last==this;
        }

        public void MarkHit(Transform target)
        {
            if(target!=null)
                lastHits[target]=this;
        }

        void Update()
        {
            // Pause freeze (native: the scene stops updating its objects in the paused state; the shared flag is Power2DSourceTruthRenderer.GlobalPaused).
            // Runs after the City controller's Update (execution order below) so the very frame the pause starts is already frozen.
            if(Power2DSourceTruthRenderer.GlobalPaused) return;
            if(!activeProjectile)
                return;

            transform.position +=
                Vector3.right*(velocityX*Time.deltaTime);

            if(owner!=null)
                owner.TryF1ProjectileContact(this);

            if(IsOutsideSourceView())
                Recycle();
        }

        bool IsOutsideSourceView()
        {
            Camera c=Camera.main;
            if(c==null)
                return false;

            Vector3 v=c.WorldToViewportPoint(transform.position);
            float mx=30f/DinoCapSourceProfileV11.ViewWidth;
            float my=30f/DinoCapSourceProfileV11.ViewHeight;

            return v.z<0f ||
                   v.x < -mx || v.x > 1f+mx ||
                   v.y < -my || v.y > 1f+my;
        }

        void Recycle()
        {
            activeProjectile=false;
            ClearSceneHitMarkers();
            gameObject.SetActive(false);
        }

        void ClearSceneHitMarkers()
        {
            var remove=new List<Transform>();
            foreach(var entry in lastHits)
                if(entry.Key==null || entry.Value==null || ReferenceEquals(entry.Value.owner,owner))
                    remove.Add(entry.Key);
            foreach(var target in remove)lastHits.Remove(target);
        }

        void OnDestroy()
        {
            ClearSceneHitMarkers();
            pool.Remove(this);
        }
    }
}
