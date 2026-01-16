using Assets.Enemies.Base.Intentions;
using Assets.Interfaces.Combat;
using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    public class AttackAction : IEnemyAction
    {
        [SerializeField] private int _damageAmount;

        public AttackAction(int damageAmount)
        {
            _damageAmount = damageAmount;
        }

        public void Execute(EnemyBase enemy)
        {
            //TODO: change when charactersParty will be aded to game
            GameObject charactersParty = null;

            if (charactersParty != null)
            {
                if (charactersParty.TryGetComponent(out IDamageable damageable))
                {
                    int finalDamage = _damageAmount > 0 ? _damageAmount : enemy.Config.BaseDamage;
                    damageable.TakeDamage(finalDamage);
                    Debug.Log($"{enemy.Name} attacks for {finalDamage} damage!");
                }
            }
            else
            {
                Debug.LogWarning($"{enemy.Name} tried to attack but found no valid targets!");
            }
        }
    }
}
