using Assets.Cards.Base.Damage;
using Assets.Targeting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Cards.Base.Targeting
{
    public readonly struct CardResolvedHit
    {
        public int StepIndex { get; }
        public CardAttackStep Step { get; }
        public ITarget Target { get; }
        public TargetingMode TargetMode { get; }

        public CardResolvedHit(int stepIndex, CardAttackStep step, ITarget target, TargetingMode targetMode)
        {
            StepIndex = stepIndex;
            Step = step;
            Target = target;
            TargetMode = targetMode;
        }

        public CardResolvedHit WithTarget(ITarget target)
        {
            return new CardResolvedHit(StepIndex, Step, target, TargetMode);
        }
    }

    public interface ICardTargetResolver
    {
        IReadOnlyList<CardResolvedHit> ResolveAttackHits(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig);
    }

    public class CardTargetResolver : ICardTargetResolver
    {
        public IReadOnlyList<CardResolvedHit> ResolveAttackHits(ITargetsProvider targetsProvider, CardConfigBaseSO cardConfig)
        {
            var resolvedHits = new List<CardResolvedHit>();

            if (targetsProvider == null || cardConfig?.AttackSteps == null || cardConfig.AttackSteps.Count == 0)
            {
                return resolvedHits;
            }

            for (int stepIndex = 0; stepIndex < cardConfig.AttackSteps.Count; stepIndex++)
            {
                CardAttackStep attackStep = cardConfig.AttackSteps[stepIndex];
                ResolveStepHits(resolvedHits, targetsProvider, stepIndex, attackStep);
            }

            return resolvedHits;
        }

        private static void ResolveStepHits(List<CardResolvedHit> resolvedHits, ITargetsProvider targetsProvider, int stepIndex, CardAttackStep attackStep)
        {
            CardDamageSO damageConfig = attackStep?.Damage;
            if (damageConfig == null || damageConfig.AttackCount <= 0)
            {
                return;
            }

            List<ITarget> aliveTargets = GetAliveTargets(targetsProvider);
            if (aliveTargets.Count == 0)
            {
                return;
            }

            switch (damageConfig.TargetMode)
            {
                case TargetingMode.Same:
                    ResolveSameTargetHits(resolvedHits, stepIndex, attackStep, damageConfig, aliveTargets);
                    break;

                case TargetingMode.All:
                    ResolveAllTargetHits(resolvedHits, stepIndex, attackStep, damageConfig, aliveTargets);
                    break;

                case TargetingMode.Random:
                    ResolveRandomTargetHits(resolvedHits, stepIndex, attackStep, damageConfig, aliveTargets);
                    break;
            }
        }

        private static void ResolveSameTargetHits(List<CardResolvedHit> resolvedHits, int stepIndex, CardAttackStep attackStep, CardDamageSO damageConfig, IReadOnlyList<ITarget> aliveTargets)
        {
            ITarget target = GetTargetsFromStartPosition(aliveTargets, damageConfig.StartPosition, 1).FirstOrDefault();
            if (target == null)
            {
                return;
            }

            for (int hitIndex = 0; hitIndex < damageConfig.AttackCount; hitIndex++)
            {
                resolvedHits.Add(new CardResolvedHit(stepIndex, attackStep, target, TargetingMode.Same));
            }
        }

        private static void ResolveAllTargetHits(List<CardResolvedHit> resolvedHits, int stepIndex, CardAttackStep attackStep, CardDamageSO damageConfig, IReadOnlyList<ITarget> aliveTargets)
        {
            for (int targetIndex = 0; targetIndex < aliveTargets.Count; targetIndex++)
            {
                ITarget target = aliveTargets[targetIndex];
                for (int hitIndex = 0; hitIndex < damageConfig.AttackCount; hitIndex++)
                {
                    resolvedHits.Add(new CardResolvedHit(stepIndex, attackStep, target, TargetingMode.All));
                }
            }
        }

        private static void ResolveRandomTargetHits(List<CardResolvedHit> resolvedHits, int stepIndex, CardAttackStep attackStep, CardDamageSO damageConfig, IReadOnlyList<ITarget> aliveTargets)
        {
            for (int hitIndex = 0; hitIndex < damageConfig.AttackCount; hitIndex++)
            {
                ITarget target = aliveTargets[Random.Range(0, aliveTargets.Count)];
                resolvedHits.Add(new CardResolvedHit(stepIndex, attackStep, target, TargetingMode.Random));
            }
        }

        private static IEnumerable<ITarget> GetTargetsFromStartPosition(IReadOnlyList<ITarget> targets, CustomTypes.StartPosition startPosition, int count)
        {
            if (count <= 0 || targets == null || targets.Count == 0)
            {
                return Enumerable.Empty<ITarget>();
            }

            return startPosition switch
            {
                CustomTypes.StartPosition.Start => targets.Take(count),
                CustomTypes.StartPosition.End => targets.Skip(Mathf.Max(0, targets.Count - count)).Take(count),
                CustomTypes.StartPosition.Center => GetTargetsFromCenter(targets, count),
                _ => targets.Take(count)
            };
        }

        private static IEnumerable<ITarget> GetTargetsFromCenter(IReadOnlyList<ITarget> targets, int count)
        {
            int centerIndex = targets.Count / 2;
            int startIndex = Mathf.Max(0, centerIndex - count / 2);
            return targets.Skip(startIndex).Take(count);
        }

        private static List<ITarget> GetAliveTargets(ITargetsProvider targetsProvider)
        {
            return targetsProvider.GetAll().Where(IsTargetAlive).ToList();
        }

        private static bool IsTargetAlive(ITarget target)
        {
            return target?.Damageable?.Health != null && target.Damageable.Health.IsAlive();
        }
    }
}
