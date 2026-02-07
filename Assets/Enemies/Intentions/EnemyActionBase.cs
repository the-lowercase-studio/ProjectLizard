using Assets.CustomTypes;
using Assets.Targeting;
using UnityEngine;

namespace Assets.Enemies.Intentions
{
    public interface IEnemyAction
    {
        int RefreshValue();

        int GetValue();

        void Execute(EnemyBase enemy);
    }

    public abstract class EnemyActionBase : IEnemyAction
    {
        [SerializeField] protected ValueRange _valueRange;
        protected int _currentValue;
        protected ITarget Target => PlayerParty.Instance;

        protected EnemyActionBase()
        {
        }

        protected EnemyActionBase(int minDamage, int maxDamage)
        {
            _valueRange = new ValueRange(minDamage, maxDamage);

            RefreshValue();
        }

        public abstract void Execute(EnemyBase enemy);

        public int GetValue() => _currentValue;

        public int RefreshValue() => _currentValue = _valueRange.GetRandomValueFromRange();
    }
}
