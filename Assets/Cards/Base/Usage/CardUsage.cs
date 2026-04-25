using Assets.Effects.Base;
using Assets.Energy;
using Assets.Cards.Base.Damage;
using Assets.Cards.Base.Targeting;
using Assets.Targeting;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Cards.Base.Usage
{
    public interface ICardUsage
    {
        event EventHandler OnCardUsage;

        void Use();
    }

    [RequireComponent(typeof(Card))]
    public class CardUsage : MonoBehaviour, ICardUsage
    {
        public event EventHandler OnCardUsage;

        [Inject] private IEnergyManager _energyManager;
        [Inject] private ITargetsProvider _targetsManager;
        [Inject] private ICardTargetResolver _cardTargetResolver;
        [Inject] private IPlayerParty _playerParty;

        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void Use()
        {
            int currentEnergyCost = _card.GetCurrentEnergyCost();
            if (currentEnergyCost <= _energyManager.CurrentEnergy)
            {
                Debug.Log($"Card {_card.name} used");

                _energyManager.DecreaseCurrentEnergy(currentEnergyCost);

                TryPlayAttackAnimation();

                ExecuteAttackFlow();

                _card.Discard();
                OnCardUsage?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Debug.Log($"No enrgy for card {_card.name} usage");
            }
        }

        private void TryPlayAttackAnimation()
        {
            if (_playerParty == null || _card?.Config == null)
            {
                return;
            }

            List<PartyCharacter> characters = _playerParty.GetAllCharacters();
            if (characters == null)
            {
                return;
            }

            foreach (PartyCharacter character in characters)
            {
                if (character != null && character.TryPlayAttackAnimationForElement(_card.Config.Element))
                {
                    break;
                }
            }
        }

        private void ExecuteAttackFlow()
        {
            IReadOnlyList<CardResolvedHit> attackPlan = ResolveAttackPlan();

            for (int i = 0; i < attackPlan.Count; i++)
            {
                CardResolvedHit hit = ResolveExecutionHitTarget(attackPlan[i]);
                if (hit.Target == null)
                {
                    continue;
                }

                if (_card.CardDamage != null && !_card.CardDamage.TryApplyDamage(hit))
                {
                    continue;
                }

                TryExecuteHitEffect(hit);
            }

            _card.ClearCachedAttackPlan();
            Debug.Log($"Card '{_card.Config.Title}' executed {attackPlan.Count} hit(s).");
        }

        private IReadOnlyList<CardResolvedHit> ResolveAttackPlan()
        {
            if (_card.CachedAttackPlan != null && _card.CachedAttackPlan.Count > 0)
            {
                return _card.CachedAttackPlan;
            }

            return _cardTargetResolver.ResolveAttackHits(_targetsManager, _card.Config);
        }

        private CardResolvedHit ResolveExecutionHitTarget(CardResolvedHit hit)
        {
            if (hit.TargetMode != TargetingMode.Random || CardDamage.IsTargetAlive(hit.Target))
            {
                return hit;
            }

            List<ITarget> aliveTargets = _targetsManager.GetAll().Where(CardDamage.IsTargetAlive).ToList();
            if (aliveTargets.Count == 0)
            {
                return hit.WithTarget(null);
            }

            if (aliveTargets.Count == 1)
            {
                return hit.WithTarget(aliveTargets[0]);
            }

            return hit.WithTarget(aliveTargets[UnityEngine.Random.Range(0, aliveTargets.Count)]);
        }

        private void TryExecuteHitEffect(CardResolvedHit hit)
        {
            EffectSO effect = hit.Step?.Effect;
            if (effect == null)
            {
                return;
            }

            if (UnityEngine.Random.value > hit.Step.GetClampedEffectChance())
            {
                return;
            }

            CardEffectContext context = new CardEffectContext
            {
                Source = gameObject,
                Position = ResolveTargetPosition(hit.Target),
                Target = hit.Target,
                TargetsProvider = _targetsManager,
                EnergyManager = _energyManager,
                StepDamage = hit.Step?.Damage?.DamageValue ?? 0
            };

            effect.Execute(context);
        }

        private static Vector3 ResolveTargetPosition(ITarget target)
        {
            if (target is Component targetComponent)
            {
                return targetComponent.transform.position;
            }

            return Vector3.zero;
        }
    }
}
