using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    // SOURCE_FAITHFUL (PROVEN): DinoCap::UIMoveCtrl semantics (sub_DA6D8) for IMGUI-drawn screens other than the main menu. The City pause menu
    // (sub_64694) builds its FX/Music toggles, Back and Menu items as UIMoveCtrl, the same control class as the main menu buttons: strict
    // original bounds, half-size pressed presentation about the centre, drag-out cancels the capture (it un-shrinks), and the release
    // dispatches once, only if still captured. One NativeMenuMoveInput per control id; call Handle() in a stable order every OnGUI.
    public sealed class NativeMoveButtonHost
    {
        readonly Dictionary<int,NativeMenuMoveInput> states=new Dictionary<int,NativeMenuMoveInput>();

        // hit: screen-space rect of the control. drawn: the rect to draw the sprite into (half-size while pressed).
        public bool Handle(Rect hit,bool enabled,out Rect drawn)
        {
            int id=GUIUtility.GetControlID(FocusType.Passive);
            if(!states.TryGetValue(id,out var state)){state=new NativeMenuMoveInput();states.Add(id,state);}
            var ev=Event.current;
            bool clicked=false;
            if(enabled && GUIUtility.hotControl==0 && ev.type==EventType.MouseDown && ev.button==0 && state.Down(hit,ev.mousePosition))
            {GUIUtility.hotControl=id;ev.Use();}
            if(GUIUtility.hotControl==id && ev.type==EventType.MouseDrag)
            {
                state.Drag(ev.mousePosition);
                if(!state.Pressed)GUIUtility.hotControl=0;
                ev.Use();
            }
            if(GUIUtility.hotControl==id && ev.type==EventType.MouseUp && ev.button==0)
            {
                clicked=state.Up() && enabled;GUIUtility.hotControl=0;ev.Use();
            }
            drawn=state.Presentation(hit);
            return clicked;
        }

        // Call when the screen closes so a stale capture can never survive into the next time it opens.
        public void CancelAll()
        {
            foreach(var s in states.Values)s.Cancel();
        }
    }
}
