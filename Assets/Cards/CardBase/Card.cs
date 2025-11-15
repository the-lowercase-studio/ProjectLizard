using Assets.Cards.CardBase;
using Assets.Interfaces;
using System;
using UnityEngine;

namespace Assets.Cards
{
    public interface ICard : IInitializableByConfig<CardConfigBaseSO>
    {
        CardConfigBaseSO Config { get; }
        Transform Visual { get; }
        ICardMovement Movement { get; }
        ICardRotation Rotation { get; }
        ICardInteractions Interactions { get; }

        event EventHandler OnCardUsage;

        byte GetCurrentEnergyCost();
    }

    public class Card : MonoBehaviour, ICard
    {
        [field: SerializeField] public CardConfigBaseSO Config { get; private set; }
        [field: SerializeField] public Transform Visual { get; private set; }
        public ICardMovement Movement { get; private set; }
        public ICardRotation Rotation { get; private set; }
        public ICardInteractions Interactions { get; private set; }

        public event EventHandler OnCardUsage;

        private void Awake()
        {
            Movement = GetComponent<ICardMovement>();
            Rotation = GetComponent<ICardRotation>();
            Interactions = GetComponent<ICardInteractions>();
        }

        public void Initialize(CardConfigBaseSO config)
        {
            if (Config == null)
            {
                Config = config;
            }
        }

        public byte GetCurrentEnergyCost()
        {
            //TODO: logic for increasing / decreasing card costs when effects are active
            return Config.StartEnergyCost;
        }
    }
}
