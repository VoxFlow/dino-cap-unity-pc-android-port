using UnityEngine;

namespace DinoCap.Gameplay.Enemies
{
    // Minimal Milestone-1 combat target. Replace inspector values with generated dinosaur.prop data.
    public sealed class EnemyCombatant : MonoBehaviour
    {
        public float maxHp = 1f;
        public float hp = 1f;
        public int reward;
        public bool IsDead { get; private set; }

        public void ResetCombatant(float recoveredHp, int recoveredReward)
        {
            maxHp = recoveredHp;
            hp = recoveredHp;
            reward = recoveredReward;
            IsDead = false;
        }

        public void ApplyDamage(float damage)
        {
            if (IsDead || damage <= 0f) return;
            hp -= damage;
            if (hp > 0f) return;
            hp = 0f;
            IsDead = true;
            SendMessage("OnRecoveredEnemyDeath", reward, SendMessageOptions.DontRequireReceiver);
        }
    }
}
