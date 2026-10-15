using DinoCap.Gameplay.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DinoCap.UI
{
    public sealed class HoldDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public PlayerController player;
        [Range(-1, 1)] public int direction;

        public void OnPointerDown(PointerEventData eventData) => SetHeld(true);
        public void OnPointerUp(PointerEventData eventData) => SetHeld(false);
        public void OnPointerExit(PointerEventData eventData) => SetHeld(false);

        private void SetHeld(bool held)
        {
            if (player == null) return;
            if (direction < 0) player.InputState.LeftHeld = held;
            if (direction > 0) player.InputState.RightHeld = held;
        }
    }
}
