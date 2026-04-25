using Assets.Effects.Base;
using Assets.Effects.StatusEffects;
using Assets.Targeting;
using Assets.Turns;

namespace Assets.Effects.InstantEffects.ExtendParalysis
{
    public class ParalysisExtension : IStatusEffectBase
    {
        public int RemainingTurns { get; }
        public EffectType EffectType => EffectType.Paralysis;
        public TurnExecutionState ExecutionState => TurnExecutionState.Instant;
        public string EffectValueDisplay => string.Empty;
        public bool CanStackValue => false;

        public EffectSO EffectData { get; protected set; }

        public ParalysisExtension(int extensionTurns)
        {
            RemainingTurns = extensionTurns;
        }

        public void Apply(ITarget target)
        { }

        public void PerformEffect()
        { }

        public void Remove()
        { }

        public void StackWith(IStatusEffectBase other)
        { }
    }
}
