using UnityEngine;
namespace DinoCap.PortV2
{
    public sealed class F2EnemyBigFireVisual : MonoBehaviour
    {
        Power2DSourceTruthRenderer visual,targetVisual;
        Transform target;
        float yOffset,sourceScale=1f;

        public static F2EnemyBigFireVisual Create(DCManifest m,int order)
        {
            GameObject g=new GameObject("Task027K2_Dahuo");
            F2EnemyBigFireVisual v=g.AddComponent<F2EnemyBigFireVisual>();
            v.visual=g.AddComponent<Power2DSourceTruthRenderer>();
            v.visual.Initialize(m); v.visual.baseSortingOrder=order;
            // Keep Dahuo's authored internal part Z within the effect's
            // actor order, rather than leaking across the flame/body bands.
            v.visual.EnableActorComposition();
            g.SetActive(false); return v;
        }

        public void Begin(Transform e,Power2DSourceTruthRenderer ev,float scale,float y)
        {
            target=e; targetVisual=ev; sourceScale=scale; yOffset=y;
            gameObject.SetActive(true); Follow();
            visual.primary.Play(visual.FindClip("DahuoAnim"),0);
        }
        public void BeginNative(Transform e,Power2DSourceTruthRenderer ev,DinoFatalDeathCore.Species species)
        {
            //12884 writes root position first. Species4 retains child scale;
            //small species then write .8 and baseline+192 (living root+240).
            float scale=sourceScale;
            float offset=0f;
            if(species==DinoFatalDeathCore.Species.K2Trex)scale=1f;
            else if(species!=DinoFatalDeathCore.Species.K4Raptor){scale=.8f;offset=-48f;}
            Begin(e,ev,scale,offset);
        }

        public void StopNow()
        {
            if(visual!=null) visual.primary.Stop();
            gameObject.SetActive(false); target=null; targetVisual=null;
        }

        void Update(){ if(gameObject.activeSelf) Follow(); }
        void Follow()
        {
            if(target==null) return;
            transform.position=target.position+Vector3.up*yOffset;
            transform.rotation=Quaternion.identity;
            visual.actorScale=sourceScale;
            if(targetVisual!=null) visual.facingRight=targetVisual.facingRight;
        }
    }
}
