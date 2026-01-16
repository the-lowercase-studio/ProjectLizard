using Assets.Enemies.Base.Intentions;
using Assets.Interfaces.Combat;
using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    public class SpecialAction : IEnemyAction
    {
        //TODO: currently same as attack action
        [SerializeField] private int _damageAmount;

        public SpecialAction(int damageAmount)
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
                    int finalDamage = _damageAmount > 0 ? _damageAmount : (enemy.Config.BaseDamage * 2);
                    damageable.TakeDamage(finalDamage);
                    Debug.Log($"{enemy.Name} uses special attack for {finalDamage} damage!");
                }
            }
            else
            {
                Debug.LogWarning($"{enemy.Name} tried to use special action but found no valid targets!");
            }
        }
    }
}
