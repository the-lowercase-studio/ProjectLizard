using Assets.CustomTypes.ValueRanges;
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
        [SerializeField] protected IntValueRange _valueRange;
        protected int _currentValue;

        protected EnemyActionBase()
        {
        }

        protected EnemyActionBase(int minDamage, int maxDamage)
        {
            _valueRange = new IntValueRange(minDamage, maxDamage);

            RefreshValue();
        }

        public abstract void Execute(IEnemyBase enemy, ITarget target);

        public int GetValue() => _currentValue;

        public int RefreshValue() => _currentValue = _valueRange.GetRandomValueFromRange();
    }
}
