using UnityEngine;

namespace DinoCap.PortV2
{
    // 614EC -> 61AD4 -> 624BC/62648/62704/62580. Native leave start
    // tuples differ from entry tuples; N2.52/native_menu_leave_tuples.json.
    public sealed class NativeMenuPrimaryLeave
    {
        static readonly float[] Delays={0f,0.1f,0.2f,0.3f,0.4f};
        static readonly Vector4[] From={new Vector4(512,156,655,239),new Vector4(500,450,430,85),
            new Vector4(512,462,241,64),new Vector4(985,739,75,65),new Vector4(500,350,512,86)};
        static readonly Vector4[] Via={new Vector4(512,-612,655,239),new Vector4(398,450,430,85),
            new Vector4(410,462,241,64),new Vector4(883,739,75,65),new Vector4(398,350,457,75)};
        static readonly Vector4[] To={Vector4.zero,new Vector4(1524,450,430,85),
            new Vector4(1536,462,241,64),new Vector4(2009,739,75,65),new Vector4(1524,350,457,75)};
        public readonly NativeUiRectTween[] Controls=new NativeUiRectTween[5];
        public readonly NativeUiRectTween Help=new NativeUiRectTween(new Vector4(866.5f,627,103,86),
            new Vector4(866.5f,595,103,86),new Vector4(866.5f,947,103,86),0.05f,0.2f);
        public bool Active {get;private set;}=true;
        float elapsed;
        readonly int completionIndex;
        public NativeMenuPrimaryLeave(int completionIndex=3){this.completionIndex=completionIndex;Create(0);}
        void Create(int index)=>Controls[index]=new NativeUiRectTween(From[index],Via[index],To[index],
            index==4?0.02f:0.05f,index==4?0.5f:0.2f);
        public void Tick(float dt)
        {
            if(!Active)return;
            elapsed+=dt;
            for(int i=0;i<Controls.Length;i++) {
                if(Controls[i]==null && elapsed>=Delays[i])Create(i);
                Controls[i]?.Tick(dt);
            }
            Help.Tick(dt);
            // 5D380 case3/event1 creates5E144 rows and removes primary controls.
            if(Controls[completionIndex]!=null && !Controls[completionIndex].Active)Active=false;
        }
    }
}
