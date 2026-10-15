using UnityEngine;

namespace DinoCap.PortV2
{
    // 5590C ->56118/11 ->56450 ->56118/12 ->LevelLeave(0.5)
    // ->5744C/57AD0 ->56118/12 ->569D0 ->56118/11 (scene_state3).
    public sealed class NativeLevelIntro
    {
        public enum Stage { BandEnter, LabelEnter, Hold, LabelLeave, BandLeave, Complete }
        public Stage CurrentStage { get; private set; }
        public NativeUiRectTween Band { get; private set; }
        public NativeUiRectTween Label { get; private set; }
        public bool Active => CurrentStage!=Stage.Complete;
        public bool LabelVisible => CurrentStage==Stage.LabelEnter || CurrentStage==Stage.Hold || CurrentStage==Stage.LabelLeave;
        float holdElapsed;
        static readonly Vector4 BandFinal=new Vector4(512,342,1024,300);
        readonly Vector4 LabelFinal;
        readonly bool labelLevelControl;

        public NativeLevelIntro(Vector4? labelFinal=null,bool labelLevelControl=true)
        {
            LabelFinal=labelFinal ?? new Vector4(523,347.5f,446,101);
            this.labelLevelControl=labelLevelControl;
            Band=new NativeUiRectTween(new Vector4(512,342,1024,0),
                new Vector4(512,342,1024,330),BandFinal,0.2f,0.05f);
        }
        // Native19A1C initializes Jungle START rect(360,297,300,101);
        // 166FC/16928 derive its centre and use the same band/tween owner.
        public static NativeLevelIntro CreateJungle()
            => new NativeLevelIntro(new Vector4(510,347.5f,300,101),false);
        Vector4 LabelAt(float x)=>new Vector4(x,LabelFinal.y,LabelFinal.z,LabelFinal.w);
        public void Tick(float dt)
        {
            switch(CurrentStage)
            {
                case Stage.BandEnter:
                    Band.Tick(dt);
                    if(!Band.Active)
                    {
                        Label=new NativeUiRectTween(LabelAt(LabelFinal.x+1024),
                            LabelAt(LabelFinal.x-102),LabelFinal,0.2f,0.05f,labelLevelControl);
                        CurrentStage=Stage.LabelEnter;
                    }
                    break;
                case Stage.LabelEnter:
                    Label.Tick(dt);
                    if(!Label.Active) { CurrentStage=Stage.Hold; holdElapsed=0f; }
                    break;
                case Stage.Hold:
                    holdElapsed+=dt;
                    if(holdElapsed>=0.5f)
                    {
                        Label=new NativeUiRectTween(LabelFinal,LabelAt(LabelFinal.x+102),
                            LabelAt(LabelFinal.x-1024),0.05f,0.2f,labelLevelControl);
                        CurrentStage=Stage.LabelLeave;
                    }
                    break;
                case Stage.LabelLeave:
                    Label.Tick(dt);
                    if(!Label.Active)
                    {
                        Band=new NativeUiRectTween(BandFinal,new Vector4(512,342,1024,330),
                            new Vector4(512,342,1024,0),0.05f,0.2f);
                        CurrentStage=Stage.BandLeave;
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
