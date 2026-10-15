using UnityEngine;

namespace DinoCap.PortV2
{
    // 610EC band -> 5D380/5 -> 5EEEC controls. BACK/GO:60D28
    // -> 5D380/6 label completion -> 5FC7C band -> 5D380/5 cleanup.
    internal sealed class NativeMenuLevelSelect
    {
        internal enum Stage { BandEnter, ControlsEnter, Shown, ControlsLeave, BandLeave, Complete }
        internal Stage CurrentStage { get; private set; }
        internal NativeUiRectTween Band { get; private set; }
        internal readonly NativeUiRectTween[] Controls=new NativeUiRectTween[5];
        internal bool Interactive=>CurrentStage==Stage.Shown;
        internal bool Visible=>CurrentStage!=Stage.Complete;
        internal bool ControlsVisible=>CurrentStage==Stage.ControlsEnter || CurrentStage==Stage.Shown || CurrentStage==Stage.ControlsLeave;
        // Label, left, right, BACK, GO. InitFunc_11 uses a 90-high menu
        // label container, unlike the 101-high gameplay label container.
        internal static readonly Rect[] Rest={new Rect(300,297,446,90),
            new Rect(20,297,67,90),new Rect(937,297,67,90),
            new Rect(0,605,154,63),new Rect(870,603,154,65)};
        static readonly Vector4 BandRest=new Vector4(512,342,1024,300);
        static Vector4 Center(Rect r)=>new Vector4(r.center.x,r.center.y,r.width,r.height);
        static Vector4 Shift(Vector4 r,float x){r.x+=x;return r;}

        internal NativeMenuLevelSelect()
        {
            Band=new NativeUiRectTween(new Vector4(512,342,1024,0),
                new Vector4(512,342,1024,330),BandRest,0.2f,0.05f);
        }
        internal void BeginLeave()
        {
            if(!Interactive) return;
            for(int i=0;i<Controls.Length;i++) {
                Vector4 rest=Center(Rest[i]);float direction=i==4?1f:-1f;
                Controls[i]=new NativeUiRectTween(rest,Shift(rest,-102*direction),
                    Shift(rest,1024*direction),0.05f,0.2f,i==0);
            }
            CurrentStage=Stage.ControlsLeave;
        }
        internal void Tick(float dt)
        {
            switch(CurrentStage) {
                case Stage.BandEnter:
                    Band.Tick(dt);
                    if(!Band.Active) {
                        for(int i=0;i<Controls.Length;i++) {
                            Vector4 rest=Center(Rest[i]);float direction=i==3?-1f:1f;
                            Controls[i]=new NativeUiRectTween(Shift(rest,1024*direction),
                                Shift(rest,-102*direction),rest,0.2f,0.05f,i==0);
                        }
                        CurrentStage=Stage.ControlsEnter;
                    }
                    break;
                case Stage.ControlsEnter:
                case Stage.ControlsLeave:
                    foreach(var control in Controls) control.Tick(dt);
                    // Native callback belongs to UILevelCtrl ID6, not a
                    // guessed wall-clock delay or the generic button tweens.
                    if(!Controls[0].Active) {
                        if(CurrentStage==Stage.ControlsEnter) CurrentStage=Stage.Shown;
                        else {
                            Band=new NativeUiRectTween(BandRest,new Vector4(512,342,1024,330),
                                new Vector4(512,342,1024,0),0.05f,0.2f);
                            CurrentStage=Stage.BandLeave;
                        }
                    }
                    break;
                case Stage.BandLeave:
                    Band.Tick(dt);
                    if(!Band.Active) CurrentStage=Stage.Complete;
                    break;
            }
        }
    }
}
