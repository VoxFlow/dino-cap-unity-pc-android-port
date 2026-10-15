using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Native sub_668FC: at contact time, test each live dino's tou anchor
    // against strict forward X bounds; group=1 collects all eligible targets.
    // Proven by Task023_BlueMeleeCombatClosure/unity_melee_contract.json.
    public static class MeleeContactCore
    {
        public static int Dispatch<T>(IEnumerable<T> targets, Vector3 playerPosition,
            bool facingRight, float range, Func<T, bool> eligible,
            Func<T, Vector3> headPosition, Action<T> applyHit)
        {
            int hits = 0;
            foreach(T target in targets)
            {
                if(!eligible(target)) continue;
                float dx = facingRight ? headPosition(target).x-playerPosition.x
                    : playerPosition.x-headPosition(target).x;
                if(dx<=0f || dx>=range) continue;
                applyHit(target);
                hits++;
            }
            return hits;
        }
    }
}
