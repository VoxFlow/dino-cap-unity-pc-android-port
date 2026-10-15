using UnityEngine;

namespace DinoCap.PortV2
{
    // Native lifeModeSceneiPad shares mainSceneiPad combat. This marker
    // selects its independent scene owners and in-memory session carry.
    public sealed class JungleBlitzController : MonoBehaviour
    {
        public const float NativeWorldRight=2000000000f;
        public int CarriedCash { get; private set; }
        public int CarriedLives { get; private set; }
        public bool HasExplicitSession { get; private set; }
        public void SetCarriedSession(int cash,int lives)
        {
            CarriedCash=cash;CarriedLives=lives;HasExplicitSession=true;
        }
    }
}
