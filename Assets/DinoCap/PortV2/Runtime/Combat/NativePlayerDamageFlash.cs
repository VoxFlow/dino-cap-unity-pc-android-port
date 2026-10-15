namespace DinoCap.PortV2
{
    //65C78: RGB only, clamped at .5/1 and symmetric four units/second.
    //65988 starts the rate at +4. Wudi expiry restores RGB, not phase rate.
    public static class NativePlayerDamageFlash
    {
        public static void Tick(ref float brightness,ref float rate,float dt)
        {
            brightness+=rate*dt;
            if(brightness>=1f){brightness=1f;rate=-4f;}
            else if(brightness<=.5f){brightness=.5f;rate=4f;}
        }
    }
}
