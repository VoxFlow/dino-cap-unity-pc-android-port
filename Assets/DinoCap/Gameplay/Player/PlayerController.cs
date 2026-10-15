using DinoCap.Core;
using DinoCap.Data;
using DinoCap.Power2DCompat;
using UnityEngine;

namespace DinoCap.Gameplay.Player
{
    [RequireComponent(typeof(Power2DAnimationPlayer), typeof(Power2DActorRenderer))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Recovered movement")]
        public float sourceMoveSpeed = 205f;

        [Header("Generated animations")]
        public Power2DAnimationAsset stand;
        public Power2DAnimationAsset move;
        public Power2DAnimationAsset melee;
        public Power2DAnimationAsset shoot;

        [Header("Generated weapon tiers")]
        public Power2DPartPatchAsset[] weaponCycle;

        public PlayerCombatState State { get; private set; } = PlayerCombatState.Stand;
        public DinoCapInputState InputState { get; } = new DinoCapInputState();

        private Power2DAnimationPlayer _animation;
        private Power2DActorRenderer _renderer;
        private int _weaponIndex;

        private void Awake()
        {
            _animation = GetComponent<Power2DAnimationPlayer>();
            _renderer = GetComponent<Power2DActorRenderer>();
            _animation.Completed += OnAnimationCompleted;
            _animation.MarkedFrameEntered += OnMarkedFrameEntered;
        }

        private void Start()
        {
            SetState(PlayerCombatState.Stand);
            ApplyWeapon();
        }

        private void Update()
        {
            HandleActions();
            HandleMovement(Time.deltaTime);
            InputState.ConsumeFrameActions();
        }

        private void HandleActions()
        {
            if (State == PlayerCombatState.Dead || State == PlayerCombatState.Hit) return;

            if (InputState.SwitchWeaponPressed)
            {
                if (weaponCycle != null && weaponCycle.Length > 0)
                {
                    _weaponIndex = (_weaponIndex + 1) % weaponCycle.Length;
                    ApplyWeapon();
                }
            }

            if (InputState.MeleePressed && melee != null)
            {
                SetState(PlayerCombatState.Melee);
                return;
            }

            if (InputState.ShootPressed && shoot != null)
            {
                SetState(PlayerCombatState.Shoot);
            }
        }

        private void HandleMovement(float dt)
        {
            if (State == PlayerCombatState.Melee || State == PlayerCombatState.Shoot || State == PlayerCombatState.Hit || State == PlayerCombatState.Dead)
                return;

            int horizontal = InputState.Horizontal;
            if (horizontal == 0)
            {
                if (State != PlayerCombatState.Stand) SetState(PlayerCombatState.Stand);
                return;
            }

            _renderer.facingLeft = horizontal < 0;
            transform.position += Vector3.right * (horizontal * sourceMoveSpeed * dt);
            if (State != PlayerCombatState.Move) SetState(PlayerCombatState.Move);
        }

        private void SetState(PlayerCombatState next)
        {
            State = next;
            Power2DAnimationAsset asset = null;
            switch (next)
            {
                case PlayerCombatState.Stand: asset = stand; break;
                case PlayerCombatState.Move: asset = move; break;
                case PlayerCombatState.Melee: asset = melee; break;
                case PlayerCombatState.Shoot: asset = shoot; break;
            }
            if (asset != null) _animation.Play(asset, true);
        }

        private void OnAnimationCompleted()
        {
            if (State == PlayerCombatState.Melee || State == PlayerCombatState.Shoot)
                SetState(InputState.Horizontal == 0 ? PlayerCombatState.Stand : PlayerCombatState.Move);
        }

        private void OnMarkedFrameEntered(int frame)
        {
            if (State == PlayerCombatState.Shoot)
            {
                // Milestone hook: projectile/ammo system plugs in here.
                SendMessage("OnRecoveredShootEvent", frame, SendMessageOptions.DontRequireReceiver);
            }
            else if (State == PlayerCombatState.Melee)
            {
                // Milestone hook: melee hit query plugs in here.
                SendMessage("OnRecoveredMeleeEvent", frame, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void ApplyWeapon()
        {
            if (weaponCycle == null || weaponCycle.Length == 0)
            {
                _renderer.SetWeaponPatch(null);
                return;
            }
            _weaponIndex = Mathf.Clamp(_weaponIndex, 0, weaponCycle.Length - 1);
            _renderer.SetWeaponPatch(weaponCycle[_weaponIndex]);
        }
    }
}
