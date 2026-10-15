using UnityEngine;
namespace DinoCap.PortV2
{
    public sealed class F2FlameVisual : MonoBehaviour
    {
        Power2DSourceTruthRenderer visual,playerVisual;
        Transform player;
        DCAnimation clip;
        bool held,tailing;
        // True while the clip in flight is the held-start clip (bound 5). Native sub_679F4 starts it sub_76980(eff,0,0,0.025,held?5:-1);
        // sub_663A0 release restarts it from the current frame with bound -1 (the tail).
        bool heldBound;

        public static F2FlameVisual Create(
            DCManifest m,Power2DSourceTruthRenderer pv,Transform p,int order)
        {
            GameObject g=new GameObject("Task027K2_F2_Huoyan");
            F2FlameVisual v=g.AddComponent<F2FlameVisual>();
            v.playerVisual=pv; v.player=p;
            v.visual=g.AddComponent<Power2DSourceTruthRenderer>();
            v.visual.Initialize(m); v.visual.baseSortingOrder=order;
            v.clip=v.visual.FindClip("huoyanAnim");
            g.SetActive(false); return v;
        }

        public void BeginOrRearmHeld()
        {
            // Task027K4 / native sub_679F4:
            // maintenance calls while already active MUST preserve the current
            // outer-envelope frame. Restart only on a true inactive->active edge.
            bool wasInactive=!gameObject.activeSelf;
            if(wasInactive)
            {
                gameObject.SetActive(true);
                F2DebugTelemetry.Log("HUOYAN_ACTIVATE",DebugStateSummary());
            }

            held=true;
            tailing=false;
            FollowSocket();

            if(wasInactive)
            {
                int oldFrame=visual.primary.Frame;
                visual.primary.PlayRange(clip,0,5);
                heldBound=true;
                F2DebugTelemetry.Log(
                    "OUTER_FRAME_RESET",
                    "oldFrame="+oldFrame+" -> 0; "+DebugStateSummary());
            }
        }

        public void EndHeld(bool tail)
        {
            if(!gameObject.activeSelf) return;
            held=false;
            if(tail && clip!=null)
            {
                // Native release preserves the effect's current outer frame;
                // it does not hard-jump to frame 6 first.
                int start=Mathf.Clamp(visual.primary.Frame,0,10);
                visual.primary.PlayRange(clip,start,10);
                heldBound=false;
                tailing=true;
            }
            else
            {
                tailing=false;
                visual.primary.Stop();
                gameObject.SetActive(false);
                F2DebugTelemetry.Log("HUOYAN_DEACTIVATE",DebugStateSummary());
            }
        }

        void Update()
        {
            if(!gameObject.activeSelf) return;
            FollowSocket();
            // NATIVE sub_12F28 (effectClassiPad kind 5, primary completion): a clip that completes on frame 5 returns (the held flame stays
            // visible, frozen on frame 5). Completing on any other frame - i.e. a release tail, INCLUDING one that was re-pressed while it
            // was still playing - hides the effect; the next frame-3 event (sub_679F4: effect not visible) then restarts it from frame 0.
            // Without this, press-release-press left a tail clip frozen on its last (tiny) frame until the next release.
            if(!visual.primary.Active && !(heldBound && visual.primary.Frame==5))
            {
                tailing=false;
                heldBound=false;
                gameObject.SetActive(false);
                F2DebugTelemetry.Log(
                    "HUOYAN_DEACTIVATE",
                    "clip-complete off frame 5 (held="+held+"); "+DebugStateSummary());
            }
        }

        public string DebugStateSummary()
        {
            string clipName=
                visual!=null && visual.primary.Clip!=null
                ? visual.primary.Clip.name
                : "<none>";

            return
                "huoyanActive="+gameObject.activeSelf+
                " held="+held+
                " tailing="+tailing+
                " outerClip="+clipName+
                " outerActive="+(visual!=null && visual.primary.Active)+
                " outerFrame="+(visual!=null ? visual.primary.Frame : -1)+
                " pos=("+
                    transform.position.x.ToString("0.00")+","+
                    transform.position.y.ToString("0.00")+")";
        }

        void FollowSocket()
        {
            Vector3 w=player.position; float r; Vector2 s;
            if(playerVisual.TryGetPartWorldSocket(
                "yyj5",0.5f,0.1f,out w,out r,out s))
                transform.position=w;
            transform.rotation=Quaternion.identity;
            visual.facingRight=playerVisual.facingRight;
            visual.actorScale=1f;
        }
    }
}
