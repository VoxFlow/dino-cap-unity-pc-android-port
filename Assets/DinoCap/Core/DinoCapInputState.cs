namespace DinoCap.Core
{
    public sealed class DinoCapInputState
    {
        public bool LeftHeld { get; set; }
        public bool RightHeld { get; set; }
        public bool MeleePressed { get; set; }
        public bool ShootPressed { get; set; }
        public bool SwitchWeaponPressed { get; set; }

        public int Horizontal
        {
            get
            {
                if (LeftHeld == RightHeld) return 0;
                return LeftHeld ? -1 : 1;
            }
        }

        public void ConsumeFrameActions()
        {
            MeleePressed = false;
            ShootPressed = false;
            SwitchWeaponPressed = false;
        }
    }
}
