using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    //13FD4/584B4: one scene-owned 40-slot ring for blood, debris,
    //flying heads and suirou. Reclaim physics owners, retain sprite/size.
    public sealed class NativeDamageParticlePool : MonoBehaviour
    {
        const int Capacity=40;
        static readonly List<NativeDamageParticlePool> pools=new List<NativeDamageParticlePool>();
        readonly GameObject[] slots=new GameObject[Capacity];
        int next;
        public static GameObject Acquire(Component source)
        {
            Transform owner=CityGridController.SceneEffectRoot(source);
            if(owner==null && source!=null)
            {
                var sandbox=Object.FindFirstObjectByType<CombatSandboxV2>();
                if(sandbox!=null && sandbox.gameObject.scene==source.gameObject.scene)owner=sandbox.transform;
            }
            int scene=source!=null?source.gameObject.scene.handle:UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
            NativeDamageParticlePool pool=null;
            for(int i=pools.Count-1;i>=0;i--)
            {
                if(pools[i]==null){pools.RemoveAt(i);continue;}
                if(pools[i].transform.parent==owner && pools[i].gameObject.scene.handle==scene)pool=pools[i];
            }
            if(pool==null)
            {
                var container=new GameObject("NativeDamageParticlePool");
                if(owner!=null)container.transform.SetParent(owner,false);
                pool=container.AddComponent<NativeDamageParticlePool>();pools.Add(pool);
                for(int i=0;i<Capacity;i++)
                {
                    var slot=new GameObject("NativeParticleSlot_"+i);
                    slot.transform.SetParent(container.transform,false);
                    slot.AddComponent<SpriteRenderer>().enabled=false;
                    slot.SetActive(false);pool.slots[i]=slot;
                }
            }
            var result=pool.slots[pool.next];pool.next=(pool.next+1)%Capacity;
            foreach(var behaviour in result.GetComponents<MonoBehaviour>())behaviour.enabled=false;
            result.SetActive(true);
            var renderer=result.GetComponent<SpriteRenderer>();renderer.enabled=renderer.sprite!=null;
            return result;
        }
    }
}
