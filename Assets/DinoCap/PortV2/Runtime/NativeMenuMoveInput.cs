using UnityEngine;

namespace DinoCap.PortV2
{
    // DA6D8: strict original bounds, half-size pressed presentation,
    // drag-out cancels capture, phase2 release dispatches once if captured.
    public sealed class NativeMenuMoveInput
    {
        Rect original;
        public bool Pressed { get; private set; }
        static bool Inside(Rect rect,Vector2 p)=>p.x>rect.xMin && p.x<rect.xMax && p.y>rect.yMin && p.y<rect.yMax;
        public bool Down(Rect rect,Vector2 point)
        {
            original=rect;Pressed=Inside(rect,point);return Pressed;
        }
        public void Drag(Vector2 point){if(Pressed && !Inside(original,point))Pressed=false;}
        public bool Up(){bool selected=Pressed;Pressed=false;return selected;}
        public void Cancel()=>Pressed=false;
        public Rect Presentation(Rect rest)
        {
            if(!Pressed)return rest;
            Rect pressed=rest;pressed.width*=0.5f;pressed.height*=0.5f;pressed.center=rest.center;
            return pressed;
        }
    }
}
