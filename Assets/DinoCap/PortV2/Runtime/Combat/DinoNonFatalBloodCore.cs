using UnityEngine;

namespace DinoCap.PortV2
{
    // Native sub_10554 G/E nonfatal blood loops; callers retain family gating.
    public static class DinoNonFatalBloodCore
    {
        public static void SpawnBallistic(Power2DSourceTruthRenderer visual, bool enabled)
        {
            if(!enabled) return;
            Vector3 head = visual.transform.position;
            visual.TryGetPartWorldAnchor("tou",out head);
            float floor = visual.transform.position.y-250f;
            for(int i=0;i<3;i++)
                DinoCapDamageParticle.SpawnOrdinaryBlood(visual,head,floor,visual.baseSortingOrder);
        }
    }
}
