using Assets.Targeting;
using Assets.Turns;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffect
    {
        string EffectName { get; }
        int RemainingTurns { get; }
        EffectType EffectType { get; }
        TurnExecutionState ExecutionState { get; }
        string EffectValueDisplay { get; }
        bool CanStackValue { get; }

        void Apply(ITarget target);

        void PerformEffect();

        void Remove();

        void StackWith(IStatusEffect other);
    }

    public struct StatusEffectConfig
    {
        public string EffectName;
        public int Turns;
        public TurnExecutionState ExecutionState;
        public bool CanStackValue;
        public EffectType EffectType;

        public StatusEffectConfig(string effectName, int turns, TurnExecutionState executionState, bool canStackValue = false, EffectType effectType = EffectType.None)
        {
            EffectName = effectName;
            Turns = turns;
            ExecutionState = executionState;
            CanStackValue = canStackValue;
            EffectType = effectType;
        }
    }

    public abstract class StatusEffectBase : IStatusEffect
    {
        public string EffectName { get; protected set; }
        public float Duration { get; protected set; }
        public int RemainingTurns { get; protected set; }
        public EffectType EffectType { get; protected set; }
        public TurnExecutionState ExecutionState { get; protected set; }
        public virtual string EffectValueDisplay { get; protected set; }
        public bool CanStackValue { get; protected set; }

        protected ITarget Target { get; private set; }

        protected StatusEffectBase(StatusEffectConfig config)
        {
            EffectName = config.EffectName;
            RemainingTurns = config.Turns;
            ExecutionState = config.ExecutionState;
            CanStackValue = config.CanStackValue;
            EffectType = config.EffectType;
            EffectValueDisplay = string.Empty;
        }

        public void Apply(ITarget target)
        {
            Target = target;

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

            Target.StatusEffectReceiver?.RemoveStatusEffect(this);
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
