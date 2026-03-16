using Assets.ElementalSystem;

namespace Assets.Effects.StatusEffects
{
    public interface IIncomingDamageModifier
    {
        int ModifyIncomingDamage(int incomingDamage, Elements damageElement);
    }
}