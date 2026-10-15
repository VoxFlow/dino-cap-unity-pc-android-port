using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    [Serializable] public class DCManifest { public string schema; public string platform; public float sourceScale; public DCAnimation[] animations; public DCPatch[] patches; public DCAtlas[] atlas; public DCEventSpec[] events; public DCCombat combat; public string[] unresolved; }
    [Serializable] public class DCAnimation { public string name; public float frameDuration; public bool loop; public DCFrame[] frames; }
    [Serializable] public class DCFrame { public int ordinal; public DCPart[] parts; }
    [Serializable] public class DCPart { public string name; public string refName; public string @ref; public float x,y,w,h,a,b,c; public int rgba; public int dBits; public string Ref { get { return string.IsNullOrEmpty(@ref) ? refName : @ref; } } }
    [Serializable] public class DCPatch { public string name; public DCPart part; }
    [Serializable] public class DCAtlas { public string @ref; public string textureResource; public int textureWidth,textureHeight; public float x,yTop,w,h; public string confidence; }
    [Serializable] public class DCEventSpec { public string animationPrefix; public int[] frames; public string action; public string confidence; }
    [Serializable] public class DCWeapon { public float atkMin,atkMax,atkDis; public bool group; }
    [Serializable] public class DCK1 { public float hp,speed; public int reward; public float attack,adis; }
    [Serializable] public class DCCombat { public float playerSpeed,playerHP; public int playerAmmoA; public DCWeapon A1,A2; public DCK1 k1; public float invulnerabilitySeconds,knockbackSpeed; }

    public static class SourceTruthLoader
    {
        public static DCManifest Load()
        {
            TextAsset t = Resources.Load<TextAsset>("DinoCapGenerated/combat_manifest");
            if (t == null) throw new InvalidOperationException("Missing generated combat_manifest. Run Dev/Tools/UnityExport/export_combat_sandbox_v2.py first.");
            DCManifest m = JsonUtility.FromJson<DCManifest>(t.text);
            if (m == null || m.schema != "DC1_COMBAT_SANDBOX_V2") throw new InvalidOperationException("Unexpected/invalid DC1 combat manifest.");
            return m;
        }
    }
}
