using Assets.Effects;

namespace Assets.Effects.StatusEffects
{
    public interface IStatusEffect
    {
        string EffectName { get; }
        int RemainingTurns { get; }
        EffectType EffectType { get; }

        void Apply(IStatusEffectReceiver target);

        void OnTurnStart();

        void OnTurnEnd();

        void Remove();
    }

    public abstract class StatusEffectBase : IStatusEffect
    {
        public string EffectName { get; protected set; }
        public float Duration { get; protected set; }
        public int RemainingTurns { get; protected set; }
        public EffectType EffectType { get; protected set; }

        protected IStatusEffectReceiver Target { get; private set; }

        protected StatusEffectBase(string effectName, byte turns, EffectType effectType = EffectType.None)
        {
            EffectName = effectName;
            RemainingTurns = turns;
            EffectType = effectType;
        }

        public void Apply(IStatusEffectReceiver target)
        {
            Target = target;

            OnApply();
        }

        public void OnTurnStart()
        {
            if (RemainingTurns > 0)
            {
                ProcessTurnEffect();
            }
        }

        public void OnTurnEnd()
        {
            if (RemainingTurns > 0)
            {
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

            Target?.RemoveStatusEffect(this);
        }

        protected abstract void OnApply();

        protected abstract void ProcessTurnEffect();

        protected abstract void OnRemove();
    }
}
