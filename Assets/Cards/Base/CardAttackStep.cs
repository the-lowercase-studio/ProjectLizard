using Assets.Cards.Base.Damage;
using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Cards.Base
{
    [System.Serializable]
    public class CardAttackStep
    {
        [field: SerializeField] public CardDamageSO Damage { get; private set; }
        [field: SerializeField] public EffectSO Effect { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float EffectChance { get; private set; } = 1f;

        public CardAttackStep()
        {
        }

        public CardAttackStep(CardDamageSO damage, EffectSO effect, float effectChance)
        {
            Damage = damage;
            Effect = effect;
            EffectChance = Mathf.Clamp01(effectChance);
        }

        public float GetClampedEffectChance()
        {
            return Mathf.Clamp01(EffectChance);
        }
    }
}