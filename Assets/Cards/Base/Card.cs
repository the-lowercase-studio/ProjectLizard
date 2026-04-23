using Assets.Cards.Base.Damage;
using Assets.Cards.Base.Targeting;
using Assets.Cards.Base.Usage;
using Assets.Interfaces;
using Assets.TweenCustom;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Cards.Base
{
    public interface ICard : IInitializableByConfig<CardConfigBaseSO>
    {
        CardConfigBaseSO Config { get; }
        GameObject Visual { get; }
        ICardMovement Movement { get; }
        ICardRotation Rotation { get; }
        ICardScaler Scaler { get; }
        ICardInteractions Interactions { get; }
        ICardUsage CardUsage { get; }
        ICardDamage CardDamage { get; }
        IReadOnlyList<CardResolvedHit> CachedAttackPlan { get; }

        int GetCurrentEnergyCost();

        event EventHandler OnCardDiscard;

        void Discard();

        void Hide();

        void Show();

        void SetCachedAttackPlan(IReadOnlyList<CardResolvedHit> attackPlan);

        void ClearCachedAttackPlan();
    }

    public class Card : MonoBehaviour, ICard
    {
        [field: SerializeField] public CardConfigBaseSO Config { get; private set; }
        [field: SerializeField] public GameObject Visual { get; private set; }

        public ICardMovement Movement { get; private set; }
        public ICardRotation Rotation { get; private set; }
        public ICardScaler Scaler { get; private set; }
        public ICardInteractions Interactions { get; private set; }
        public ICardUsage CardUsage { get; private set; }
        public ICardDamage CardDamage { get; private set; }
        public IReadOnlyList<CardResolvedHit> CachedAttackPlan { get; private set; } = new List<CardResolvedHit>();

        public event EventHandler OnCardDiscard;

        private void Awake()
        {
            Movement = GetComponent<ICardMovement>();
            Rotation = GetComponent<ICardRotation>();
            Scaler = GetComponent<ICardScaler>();
            Interactions = GetComponent<ICardInteractions>();
            CardUsage = GetComponent<ICardUsage>();
            CardDamage = GetComponent<ICardDamage>();
        }

        public void Initialize(CardConfigBaseSO config)
        {
            Config = config;
        }

        public int GetCurrentEnergyCost()
        {
            //TODO: logic for increasing / decreasing card costs when effects are active
            return Config.StartEnergyCost;
        }

        public void Discard()
        {
            StopTweensInChildren();
            ClearCachedAttackPlan();

            OnCardDiscard?.Invoke(this, EventArgs.Empty);

            Destroy(Visual);
            Destroy(gameObject);
        }

        public void Show()
        {
            Visual.SetActive(true);
        }

        public void Hide()
        {
            StopTweensInChildren();

            Visual.SetActive(false);
        }

        public void SetCachedAttackPlan(IReadOnlyList<CardResolvedHit> attackPlan)
        {
            CachedAttackPlan = attackPlan ?? new List<CardResolvedHit>();
        }

        public void ClearCachedAttackPlan()
        {
            CachedAttackPlan = new List<CardResolvedHit>();
        }

        private void StopTweensInChildren()
        {
            foreach (var tweenUser in GetComponentsInChildren<ITweenUser>())
            {
                tweenUser.StopTweens();
            }
        }
    }
}
