using Assets.Enemies.Base;
using Assets.Enemies.Intentions;
using Assets.Targeting;
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

        public override void Execute(IEnemyBase enemy, ITarget target)
        {
            if (target != null)
            {
                if (target.Damageable != null)
                {
                    target.Damageable.TakeDamage(_currentValue);
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
