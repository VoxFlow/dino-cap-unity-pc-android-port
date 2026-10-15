using UnityEngine;

namespace DinoCap.PortV2
{
    // Native5905C ->56118/15 ->55B1C/8 ->55F6C ->56118/15
    // ->56ACC ->56118/16 ->DieEnd. Geometry stores centers, not top-left.
    public sealed class NativeDeathPresentation
    {
        public enum Stage { TextPop, BlackFade, TextRise, LivesEnter, DieEndWait, Complete }
        public Stage CurrentStage { get; private set; }
        public NativeUiRectTween Text { get; private set; }
        public NativeUiRectTween Icon { get; private set; }
        public NativeUiRectTween Digit { get; private set; }
        public bool Active => CurrentStage!=Stage.Complete;
        public float BlackAlphaByte { get; private set; }
        public int DisplayedBlackAlphaByte { get; private set; }
        public bool LivesVisible => Icon!=null;
        readonly float digitWidth;
        readonly bool showLives;
        float endElapsed;
        static readonly Vector4 TextRest=new Vector4(512,384,412,156);

        public NativeDeathPresentation(float initialSceneAlpha,float digitWidth,bool showLives=true)
        {
            BlackAlphaByte=initialSceneAlpha;
            this.digitWidth=digitWidth;this.showLives=showLives;
            var full=new Vector4(512,384,1024,768);
            Text=new NativeUiRectTween(full,full,TextRest,.01f,.2f);
        }
        public void Tick(float dt)
        {
            switch(CurrentStage)
            {
                case Stage.TextPop:
                    Text.Tick(dt);
                    if(!Text.Active)CurrentStage=Stage.BlackFade;
                    break;
                case Stage.BlackFade:
                    BlackAlphaByte+=192f*dt;
                    if(BlackAlphaByte>255f) {
                        Text=new NativeUiRectTween(TextRest,TextRest,new Vector4(512,108,412,156),.05f,.5f);
                        CurrentStage=Stage.TextRise;
                    }
                    else DisplayedBlackAlphaByte=((int)BlackAlphaByte)&255;
                    break;
                case Stage.TextRise:
                    Text.Tick(dt);
                    if(!Text.Active) {
                        //Jungle vtable+84 dispatches16A30 results instead
                        //of City56ACC lives entry/DieEnd countdown.
                        if(!showLives){CurrentStage=Stage.Complete;break;}
                        Icon=new NativeUiRectTween(new Vector4(430.5f,1336.5f,161,121),
                            new Vector4(430.5f,492.5f,161,121),new Vector4(430.5f,568.5f,161,121),.2f,.05f);
                        float x=520+digitWidth*.5f;
                        Digit=new NativeUiRectTween(new Vector4(x,1359.5f,digitWidth,53),
                            new Vector4(x,515.5f,digitWidth,53),new Vector4(x,591.5f,digitWidth,53),.2f,.05f);
                        CurrentStage=Stage.LivesEnter;
                    }
                    break;
                case Stage.LivesEnter:
                    Icon.Tick(dt);Digit.Tick(dt);
                    if(!Icon.Active) {endElapsed=0;CurrentStage=Stage.DieEndWait;}
                    break;
                case Stage.DieEndWait:
                    endElapsed+=dt;
                    if(endElapsed>=1f)CurrentStage=Stage.Complete;
                    break;
            }
        }
    }
}
