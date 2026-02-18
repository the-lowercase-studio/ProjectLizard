using Assets.Enemies.Base;
using Assets.Enemies.Intentions;
using Assets.ShieldSystem;
using Assets.Targeting;
using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    [IntentionType(IntentionType.Defense)]
    public class DefenseAction : EnemyActionBase
    {
        public DefenseAction()
        { }

        public DefenseAction(int minDamage, int maxDamage)
            : base(minDamage, maxDamage)
        { }

        public override void Execute(IEnemyBase enemy, ITarget target)
        {
            if (enemy is IShielded shielded && shielded.ShieldReceiver != null)
            {
                shielded.ShieldReceiver.AddShield(_currentValue);
                Debug.Log($"{enemy.Name} gains {_currentValue} shield!");
            }
            else
            {
                Debug.LogWarning($"{enemy.Name} tried to gain shield but has no Shield component!");
            }
        }
    }
}
