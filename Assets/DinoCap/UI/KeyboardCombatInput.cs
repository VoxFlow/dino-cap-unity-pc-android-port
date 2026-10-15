using DinoCap.Gameplay.Player;
using UnityEngine;

namespace DinoCap.UI
{
    // Development/PC adapter. Mobile touch buttons feed the same DinoCapInputState.
    public sealed class KeyboardCombatInput : MonoBehaviour
    {
        public PlayerController player;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode melee = KeyCode.J;
        public KeyCode shoot = KeyCode.K;
        public KeyCode switchWeapon = KeyCode.L;

        private void Update()
        {
            if (player == null) return;
            player.InputState.LeftHeld = Input.GetKey(left);
            player.InputState.RightHeld = Input.GetKey(right);
            if (Input.GetKeyDown(melee)) player.InputState.MeleePressed = true;
            if (Input.GetKeyDown(shoot)) player.InputState.ShootPressed = true;
            if (Input.GetKeyDown(switchWeapon)) player.InputState.SwitchWeaponPressed = true;
        }
    }
}
