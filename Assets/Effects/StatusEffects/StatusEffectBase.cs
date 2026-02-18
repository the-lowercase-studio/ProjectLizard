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

        void Apply(ITarget target);

        void PerformEffect();

        void Remove();
    }

    public abstract class StatusEffectBase : IStatusEffect
    {
        public string EffectName { get; protected set; }
        public float Duration { get; protected set; }
        public int RemainingTurns { get; protected set; }
        public EffectType EffectType { get; protected set; }
        public TurnExecutionState ExecutionState { get; protected set; }
        public virtual string EffectValueDisplay { get; protected set; }

        protected ITarget Target { get; private set; }

        protected StatusEffectBase(string effectName, byte turns, TurnExecutionState executionState, EffectType effectType = EffectType.None)
        {
            EffectName = effectName;
            RemainingTurns = turns;
            ExecutionState = executionState;
            EffectType = effectType;
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

        protected abstract void OnApply();

        protected abstract void ProcessTurnEffect();

        protected abstract void OnRemove();
    }
}
