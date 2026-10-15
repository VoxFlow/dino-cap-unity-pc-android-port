using UnityEngine;

namespace DinoCap.PortV2
{
    public static class NativeJungleResults
    {
        //16A30 source stores top-left rectangles, UILabel uses same font
        //and tracking as the native HUD time counter.
        public static readonly Rect TimeTitle=new Rect(150,399,306,36);
        public static readonly Rect TimeValue=new Rect(485,389,200,60);
        public static readonly Rect KillTitle=new Rect(155,500,306,36);
        public static readonly Rect KillValue=new Rect(485,489,200,60);
        public static readonly Rect MoneyTitle=new Rect(150,600,306,36);
        public static readonly Rect MoneyValue=new Rect(485,590,200,60);
        public static NativeUiRectTween CreateDone()
        {
            //17480 exact centers/sizes and .2/.05 UIMoveCtrl phases.
            return new NativeUiRectTween(new Vector4(860,660,18.3f,6),
                new Vector4(860,660,201.3f,66),new Vector4(860,660,183,60),.2f,.05f);
        }
    }
}
