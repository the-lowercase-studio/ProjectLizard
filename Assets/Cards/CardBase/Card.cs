using Assets.Cards.CardBase;
using Assets.Interfaces;
using System;
using UnityEngine;

namespace Assets.Cards
{
    public interface ICard : IInitializableByConfig<CardConfigBaseSO>
    {
        ICardMovement Movement { get; }
        ICardRotation Rotation { get; }
        CardConfigBaseSO Config { get; }

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

        private void Start()
        {
            Interactions.OnClick += Interactions_OnClick;
            Interactions.OnHoverStart += Interactions_OnHoverStart;
            Interactions.OnHoverEnd += Interactions_OnHoverEnd;
            Interactions.OnDragStart += Interactions_OnDragStart;
            Interactions.OnDragEnd += Interactions_OnDragEnd;
            Interactions.UpdateEventHandlers();
        }

        private void Interactions_OnHoverStart(object sender, UnityEngine.EventSystems.PointerEventData e)
        {
            Debug.Log($"{sender} on hover");
            Movement.MoveCardUp();
        }

        private void Interactions_OnHoverEnd(object sender, UnityEngine.EventSystems.PointerEventData e)
        {
            Debug.Log($"{sender} on hover exit");
            Movement.SetVisualRectAnchoredPositionToPrevPosition();
        }

        private void Interactions_OnDragEnd(object sender, UnityEngine.EventSystems.PointerEventData e)
        {
            Debug.Log($"{sender} on drag end");
        }

        private void Interactions_OnDragStart(object sender, UnityEngine.EventSystems.PointerEventData e)
        {
            Debug.Log($"{sender} on drag start");
        }

        private void Interactions_OnClick(object sender, UnityEngine.EventSystems.PointerEventData e)
        {
            Debug.Log($"{sender} on click");
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
