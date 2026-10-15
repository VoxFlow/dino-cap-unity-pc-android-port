using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// Task024D source-profile bundle.
    /// Gameplay resolution never selects or modifies these values.
    /// V11 intentionally uses the complete shipped iPad profile.
    /// </summary>
    public static class DinoCapSourceProfileV11
    {
        public const string Id = "ipad";

        public const float ViewWidth = 1024f;
        public const float ViewHeight = 768f;
        public const float SpriteDeviceScale = 1f;
        public const float FloorYAnchor = -104f;

        public const float CameraKRight = 0.45f;
        public const float CameraKLeft = 0.55f;
        // C0CE4 registers playerClassiPad ->658E4 ->65674:315.
        // D22FC:195 belongs to playerClass (phone); prior labels were reversed.
        public const float CameraPanSpeed = 315f;

        public const float PlayerSpeed = 205f;

        // Task026B: iPad City Type-1 / BLUE / k3 row.
        public const float Type1HP = 5f;
        public const float Type1Speed = 77f;
        public const float Type1Attack = 10f;
        public const float Type1AdisBase = 190f;
        public const int Type1Reward = 7;
        public const int Type1Frequency = 4;

        public const float RusherHP = 1f;
        public const float RusherSpeed = 308f;
        public const float RusherAttack = 7f;
        public const float RusherAdisBase = 190f;
        public const int RusherReward = 7;
        public const int AdisJitterExclusiveMax = 20;

        public static void ValidateManifest(DCManifest m)
        {
            if (m == null)
                throw new InvalidOperationException("V11 profile validation: manifest is null.");

            if (!string.Equals(m.platform, Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "V11 MIXED-PROFILE REFUSAL: expected manifest.platform='ipad', got '" +
                    m.platform + "'.");

            if (Mathf.Abs(m.sourceScale - SpriteDeviceScale) > 0.0001f)
                throw new InvalidOperationException(
                    "V11 MIXED-PROFILE REFUSAL: iPad gameplay requires sourceScale=1.0, got " +
                    m.sourceScale.ToString("0.###") + ".");

            if (Mathf.Abs(CameraKRight - 0.45f) > 0.0001f || Mathf.Abs(CameraKLeft - 0.55f) > 0.0001f || Mathf.Abs(CameraPanSpeed - 315f) > 0.0001f)
                throw new InvalidOperationException(
                    "V12 camera source contradiction: iPad requires kRight=0.45, kLeft=0.55, panSpeed=315.");

            if (Mathf.Abs(m.combat.playerSpeed - PlayerSpeed) > 0.0001f)
                throw new InvalidOperationException(
                    "V11 source contradiction: player speed must be 205.");

            if (Mathf.Abs(m.combat.k1.speed - RusherSpeed) > 0.0001f ||
                Mathf.Abs(m.combat.k1.adis - RusherAdisBase) > 0.0001f ||
                Mathf.Abs(m.combat.k1.attack - RusherAttack) > 0.0001f ||
                Mathf.Abs(m.combat.k1.hp - RusherHP) > 0.0001f)
                throw new InvalidOperationException(
                    "V11 source contradiction: k1_move/k4-ref rusher must use iPad City type-2 row " +
                    "(HP1 SPD308 ATK7 ADIS190).");
        }
    }
}
