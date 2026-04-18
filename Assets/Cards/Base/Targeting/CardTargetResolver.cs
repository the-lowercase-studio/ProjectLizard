using Assets.Cards.Base.Damage;
using Assets.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Cards.Base.Targeting
{
    public readonly struct CardDamageTargetSelection
    {
        public ITarget Target { get; }
        public int HitCount { get; }

        public CardDamageTargetSelection(ITarget target, int hitCount)
        {
            Target = target;
            HitCount = hitCount;
        }
    }

    public interface ICardTargetResolver
    {
        IReadOnlyList<CardDamageTargetSelection> ResolveDamageTargets(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig);

        ITarget ResolveEffectTarget(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig);
    }

    public class CardTargetResolver : ICardTargetResolver
    {
        public IReadOnlyList<CardDamageTargetSelection> ResolveDamageTargets(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig)
        {
            var resolvedTargets = new List<CardDamageTargetSelection>();

            if (targetsProvider == null)
            {
                return resolvedTargets;
            }

            CardDamageSO damageConfig = cardConfig?.Damage;
            if (damageConfig == null)
            {
                return resolvedTargets;
            }

            switch (damageConfig.TargetMode)
            {
                case TargetingMode.Same:
                    ITarget sameTarget = targetsProvider
                        .GetFromStartPosition(damageConfig.StartPosition, 1)
                        .FirstOrDefault();

                    if (sameTarget != null)
                    {
                        resolvedTargets.Add(new CardDamageTargetSelection(sameTarget, damageConfig.AttackCount));
                    }
                    break;

                case TargetingMode.Other:
                    foreach (ITarget target in targetsProvider.GetFromStartPosition(damageConfig.StartPosition, damageConfig.AttackCount))
                    {
                        resolvedTargets.Add(new CardDamageTargetSelection(target, 1));
                    }
                    break;
            }

            return resolvedTargets;
        }

        public ITarget ResolveEffectTarget(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig)
        {
            if (targetsProvider == null || cardConfig?.Effects == null || cardConfig.Effects.Count == 0)
            {
                return null;
            }

            // Keep effect targeting aligned with current gameplay rule until effect-specific targeting is introduced.
            return targetsProvider.GetFirst();
        }
    }
}
