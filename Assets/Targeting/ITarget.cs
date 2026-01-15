using Assets.Effects.StatusEffects;
using Assets.Interfaces.Combat;

namespace Assets.Targeting
{
    public interface ITarget
    {
        string Name { get; }
        IDamageable Damageable { get; }
        IStatusEffectReceiver StatusEffectReceiver { get; }
    }
}
