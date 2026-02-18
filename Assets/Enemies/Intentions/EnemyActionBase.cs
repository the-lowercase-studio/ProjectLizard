using Assets.CustomTypes;
using Assets.Enemies.Base;
using Assets.Targeting;
using UnityEngine;

namespace Assets.Enemies.Intentions
{
    public interface IEnemyAction
    {
        int RefreshValue();

        int GetValue();

        void Execute(IEnemyBase enemy, ITarget target);
    }

    public abstract class EnemyActionBase : IEnemyAction
    {
        [SerializeField] protected ValueRange _valueRange;
        protected int _currentValue;

        protected EnemyActionBase()
        {
        }

        protected EnemyActionBase(int minDamage, int maxDamage)
        {
            _valueRange = new ValueRange(minDamage, maxDamage);

            RefreshValue();
        }

        public abstract void Execute(IEnemyBase enemy, ITarget target);

        public int GetValue() => _currentValue;

        public int RefreshValue() => _currentValue = _valueRange.GetRandomValueFromRange();
    }
}
