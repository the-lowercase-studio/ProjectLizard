using Assets.Enemies.Intentions;
using Assets.Interfaces.Combat;
using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    [IntentionType(IntentionType.Attack)]
    public class AttackAction : EnemyActionBase
    {
        public AttackAction()
        { }

        public AttackAction(int minDamage, int maxDamage)
            : base(minDamage, maxDamage)
        { }

        public override void Execute(EnemyBase enemy)
        {
            //TODO: change when charactersParty will be aded to game
            GameObject charactersParty = null;

            if (charactersParty != null)
            {
                if (charactersParty.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_currentValue);
                    Debug.Log($"{enemy.Name} attacks for {_currentValue} damage!");
                }
            }
            else
            {
                Debug.LogWarning($"{enemy.Name} tried to attack but found no valid targets!");
            }
        }
    }
}
