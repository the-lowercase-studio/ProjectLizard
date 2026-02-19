using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.StatusEffects
{
    [CreateAssetMenu(fileName = "New Custom Effect", menuName = "Scriptable Objects/Cards/Effects/Custom Effect")]
    public class CustomEffectSO : EffectSO
    {
        [field: SerializeField] public GameObject CustomBehaviorPrefab { get; private set; }

        public override void Execute(CardEffectContext context)
        {
            if (CustomBehaviorPrefab != null && CustomBehaviorPrefab.TryGetComponent(out ICustomCardEffect customEffect))
            {
                customEffect.ExecuteCustomEffect(context);
            }
        }
    }

    public interface ICustomCardEffect
    {
        void ExecuteCustomEffect(CardEffectContext context);
    }
}
