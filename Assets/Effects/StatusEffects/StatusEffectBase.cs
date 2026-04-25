using Assets.Effects.Base;
using Assets.Targeting;
using Assets.Turns;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffect
    {
        int RemainingTurns { get; }
        EffectType EffectType { get; }
        TurnExecutionState ExecutionState { get; }
        string EffectValueDisplay { get; }
        bool CanStackValue { get; }
        EffectSO EffectData { get; }

        void Apply(ITarget target);

        void PerformEffect();

        void Remove();

        void StackWith(IStatusEffect other);
    }

    public abstract class StatusEffectBase : IStatusEffect
    {
        public float Duration { get; protected set; }
        public int RemainingTurns { get; protected set; }
        public EffectType EffectType { get; protected set; }
        public TurnExecutionState ExecutionState { get; protected set; }
        public virtual string EffectValueDisplay { get; protected set; }
        public bool CanStackValue { get; protected set; }
        protected ITarget target;
        public EffectSO EffectData { get; protected set; }

        protected StatusEffectBase(EffectSO effectSO)
        {
            EffectData = effectSO;
            RemainingTurns = effectSO.TurnDuration;
            ExecutionState = effectSO.ExecutionState;
            CanStackValue = effectSO.CanStackValue;
            EffectType = effectSO.EffectType;
            EffectValueDisplay = string.Empty;
        }

        public void Apply(ITarget target)
        {
            this.target = target;

            OnApply();
        }

        public void PerformEffect()
        {
            if (RemainingTurns > 0)
            {
                ProcessTurnEffect();

                RemainingTurns--;

                if (RemainingTurns == 0)
                {
                    Remove();
                }
            }
        }

        public void Remove()
        {
            OnRemove();

            target.StatusEffectReceiver?.RemoveStatusEffect(this);
        }

        public void StackWith(IStatusEffect other)
        {
            RemainingTurns += other.RemainingTurns;

            if (CanStackValue)
            {
                StackValue(other);
            }

            UpdateEffectValueDisplay();
        }

        protected virtual void StackValue(IStatusEffect other)
        {
        }

        protected virtual void UpdateEffectValueDisplay()
        {
        }

        protected abstract void OnApply();

        protected abstract void ProcessTurnEffect();

        protected abstract void OnRemove();
    }
}
