using DinoCap.Core;
using DinoCap.Gameplay.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DinoCap.UI
{
    public sealed class CombatActionButton : MonoBehaviour, IPointerDownHandler
    {
        public PlayerController player;
        public DinoCapAction action;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (player == null) return;
            switch (action)
            {
                case DinoCapAction.Melee:
                    player.InputState.MeleePressed = true;
                    break;
                case DinoCapAction.Shoot:
                    player.InputState.ShootPressed = true;
                    break;
                case DinoCapAction.SwitchWeapon:
                    player.InputState.SwitchWeaponPressed = true;
                    break;
            }
        }
    }
}
