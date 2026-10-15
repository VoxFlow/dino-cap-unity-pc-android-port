using DinoCap.Gameplay.Player;
using DinoCap.Power2DCompat;
using UnityEngine;

namespace DinoCap.Debugging
{
    public sealed class CombatSandboxDebugHud : MonoBehaviour
    {
        public PlayerController player;
        public Power2DAnimationPlayer animationPlayer;
        public bool visible = true;

        private void OnGUI()
        {
            if (!visible || player == null || animationPlayer == null) return;
            GUILayout.BeginArea(new Rect(8, 8, 380, 180), GUI.skin.box);
            GUILayout.Label("Dino Cap Combat Sandbox");
            GUILayout.Label("Player state: " + player.State);
            GUILayout.Label("Input horizontal: " + player.InputState.Horizontal);
            GUILayout.Label("Animation: " + (animationPlayer.Current != null ? animationPlayer.Current.sourceName : "<none>"));
            GUILayout.Label("Frame: " + animationPlayer.FrameIndex);
            GUILayout.Label("A/D move | J melee | K shoot | L switch");
            GUILayout.EndArea();
        }
    }
}
