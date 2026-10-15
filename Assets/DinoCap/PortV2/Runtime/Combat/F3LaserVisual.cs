using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task027M.2: the F3 (jiguang / laser) beam. Source: sub_67CE8 -
    /// a persistent effectClassiPad playing 'JiguangAnim' (jiguang.a2b, 11f,
    /// single part com1 -> 'gang1' bar), anchored to the player weapon socket
    /// 'yyj5', draw key 13, held frames 0..6 then a 4..6 idle loop
    /// (sub_12F28 kind-4 held branch), release winds down current..end, then the
    /// effect self-deactivates. sub_17C840 stretches the bar from the socket
    /// toward a far point - reproduced here by the authored 1024-unit part width
    /// plus the renderer facing mirror (NOT a LineRenderer; NOT used for hit
    /// detection - the native cone test in CombatSandboxV2 owns that).
    /// Mirrors F2FlameVisual's lifetime shape; F2 code is untouched.
    /// </summary>
    public sealed class F3LaserVisual : MonoBehaviour
    {
        Power2DSourceTruthRenderer visual, playerVisual;
        Transform player;
        DCAnimation clip;
        bool held, tailing;

        const int HeldLastFrame = 6;     // sub_67CE8: sub_76980(effect,0,0,0.025, held?6:-1)
        const int HeldLoopFirst = 4;     // sub_12F28 kind-4 held: sub_76980(effect,-1,4,0.025,6)
        const int ReleaseLastFrame = 10; // JiguangAnim wind-down runs to the last frame

        public static F3LaserVisual Create(
            DCManifest m, Power2DSourceTruthRenderer pv, Transform p, int order)
        {
            GameObject g = new GameObject("Task027M_F3_Jiguang");
            F3LaserVisual v = g.AddComponent<F3LaserVisual>();
            v.playerVisual = pv;
            v.player = p;
            v.visual = g.AddComponent<Power2DSourceTruthRenderer>();
            v.visual.Initialize(m);
            v.visual.baseSortingOrder = order;
            v.clip = v.visual.FindClip("JiguangAnim");
            g.SetActive(false);
            return v;
        }

        public void BeginOrRearmHeld()
        {
            bool wasInactive = !gameObject.activeSelf;
            if (wasInactive)
                gameObject.SetActive(true);

            held = true;
            tailing = false;
            FollowSocket();

            if (wasInactive)
                visual.primary.PlayRange(clip, 0, HeldLastFrame);
        }

        public void EndHeld(bool tail)
        {
            if (!gameObject.activeSelf) return;
            held = false;
            if (tail && clip != null)
            {
                int start = Mathf.Clamp(visual.primary.Frame, 0, ReleaseLastFrame);
                visual.primary.PlayRange(clip, start, ReleaseLastFrame);
                tailing = true;
            }
            else
            {
                tailing = false;
                visual.primary.Stop();
                gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (!gameObject.activeSelf) return;
            FollowSocket();

            if (held)
            {
                // sub_12F28 kind-4 held: once the outer envelope reaches frame 6
                // it idle-loops 4..6 while the beam is sustained.
                if (!visual.primary.Active)
                    visual.primary.PlayRangeLooping(clip, HeldLoopFirst, HeldLastFrame);   // infinite 4..6 loop, no re-seed per cycle (sub_12F28 kind 4)
            }
            else if (tailing && !visual.primary.Active)
            {
                tailing = false;
                gameObject.SetActive(false);
            }
        }

        void FollowSocket()
        {
            Vector3 w = player.position;
            float r;
            Vector2 s;
            // sub_67CE8 socket offset: x = 0.5*socketW ; y = -0.15*(0.5*socketH).
            if (playerVisual.TryGetPartWorldSocket("yyj5", 0.5f, -0.075f, out w, out r, out s))
                transform.position = w;
            transform.rotation = Quaternion.identity;
            visual.facingRight = playerVisual.facingRight;
            visual.actorScale = 1f;
        }

        public string DebugStateSummary()
        {
            string clipName =
                visual != null && visual.primary.Clip != null
                    ? visual.primary.Clip.name
                    : "<none>";
            return
                "jiguangActive=" + gameObject.activeSelf +
                " held=" + held +
                " tailing=" + tailing +
                " clip=" + clipName +
                " frame=" + (visual != null ? visual.primary.Frame : -1) +
                " pos=(" +
                    transform.position.x.ToString("0.00") + "," +
                    transform.position.y.ToString("0.00") + ")";
        }
    }
}
