using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Small source-style effect pool for Task027 qh/qx play-once effects.
    /// Cleanup is animation-complete, not a guessed timer.
    /// </summary>
    public sealed class WeaponEffectAnimation : MonoBehaviour
    {
        static readonly List<WeaponEffectAnimation> pool =
            new List<WeaponEffectAnimation>();

        Power2DSourceTruthRenderer visual;
        DCManifest manifest;

        public static WeaponEffectAnimation Spawn(
            DCManifest data,
            string clip,
            Vector3 pos,
            float rotationDeg,
            bool facingRight,
            float actorScale,
            int sortingOrder)
        {
            WeaponEffectAnimation fx=null;
            for(int i=0;i<pool.Count;i++)
            {
                if(pool[i]!=null && !pool[i].gameObject.activeSelf)
                { fx=pool[i]; break; }
            }

            if(fx==null)
            {
                GameObject g=new GameObject("Task027_Effect");
                fx=g.AddComponent<WeaponEffectAnimation>();
                fx.manifest=data;
                fx.visual=g.AddComponent<Power2DSourceTruthRenderer>();
                fx.visual.Initialize(data);
                pool.Add(fx);
            }

            fx.gameObject.SetActive(true);
            fx.transform.position=pos;
            fx.transform.rotation=Quaternion.Euler(0f,0f,rotationDeg);
            fx.visual.facingRight=facingRight;
            fx.visual.actorScale=actorScale;
            fx.visual.baseSortingOrder=sortingOrder;
            fx.visual.primary.Play(fx.visual.FindClip(clip),0);
            fx.visual.secondary.Stop();
            return fx;
        }

        void Update()
        {
            if(visual!=null &&
               visual.primary.Clip!=null &&
               !visual.primary.Active)
                gameObject.SetActive(false);
        }
    }
}
