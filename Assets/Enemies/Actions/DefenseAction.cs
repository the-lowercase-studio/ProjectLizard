using Assets.Enemies.Intentions;
using Assets.ShieldSystem;
using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    [IntentionType(IntentionType.Defense)]
    public class DefenseAction : IEnemyAction
    {
        [SerializeField] private int _shieldAmount;

        public DefenseAction()
        { }

        public DefenseAction(int shieldAmount)
        {
            _shieldAmount = shieldAmount;
        }

        public void Execute(EnemyBase enemy)
        {
            if (_shieldAmount > 0 && enemy.Health.IsAlive())
            {
                if (enemy is IShielded shielded && shielded.Shield != null)
                {
                    shielded.Shield.AddShield(_shieldAmount);
                    Debug.Log($"{enemy.Name} gains {_shieldAmount} shield!");
                }
                else
                {
                    Debug.LogWarning($"{enemy.Name} tried to gain shield but has no Shield component!");
                }
            }
            else
            {
                Debug.Log($"{enemy.Name} takes a defensive stance!");
            }
        }
    }
}
