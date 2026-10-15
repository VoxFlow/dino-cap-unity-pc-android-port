using System.Collections.Generic;

namespace DinoCap.PortV2
{
    //Explicit compatibility repair of original K4's missing nested sequences.
    //Use the eleven shipped skeletal frames on the existing k4_be pose rig;
    //do not synthesize animation frames or alter the combat/phase clock.
    public static class K4XrayCompatibility
    {
        static readonly Dictionary<string,string> skeletal=new Dictionary<string,string>
        {
            {"njz","njzs"},{"njzh","njzs"},
            {"ntx","ntxs"},{"ntxh","ntxs"},
            {"nts","ntss"},{"ntsh","ntss"},
            {"ns","nss"},{"nsh","nss"},
            {"st","sts"},{"sth","sts"},
            {"wjz","wjzs"},{"wjzh","wjzs"},
            {"wtx","wtxs"},{"wtxh","wtxs"},
            {"wts","wtss"},{"wtsh","wtss"},
            {"ws","wss"},{"wsh","wss"},
            {"t","ts"},{"tjy","ts"},{"tjyh","ts"},{"tgj","ts"},{"tbz","ts"},
            {"zb","zbs"},{"zbh","zbs"}
        };
        public static bool TryGetSkeletalRef(string bodyRef,out string skeletonRef)
            =>skeletal.TryGetValue(bodyRef,out skeletonRef);
    }
}
