using System;

namespace Assets.Cards.Base.Interaction
{
    public interface ICardInteractionStateMachine
    {
        CardState CurrentState { get; }

        bool CanTransitionToHovered();

        bool CanTransitionToDragged();

        bool CanTransitionToClicked();

        bool CanTransitionToReturningToHand();

        bool CanTransitionToNone();

        bool TryTransitionToHovered(Action onTransition);

        bool TryTransitionToDragged(Action onTransition);

        bool TryTransitionToClicked(Action onTransition);

        bool TryTransitionToReturningToHand(Action onTransition);

        bool TryTransitionToNone(Action onTransition);

        void ForceTransition(CardState targetState, Action onTransition = null);
    }

    public sealed class CardInteractionStateMachine : ICardInteractionStateMachine
    {
        private readonly Func<bool> _isAnyCardBeingDragged;

        public CardState CurrentState { get; private set; } = CardState.None;

        public CardInteractionStateMachine(Func<bool> isAnyCardBeingDragged)
        {
            _isAnyCardBeingDragged = isAnyCardBeingDragged;
        }

        public bool CanTransitionToHovered()
        {
            return CurrentState == CardState.None && !_isAnyCardBeingDragged();
        }

        public bool CanTransitionToDragged()
        {
            return CurrentState == CardState.Hovered
                || CurrentState == CardState.ReturningToHand;
        }

        public bool CanTransitionToClicked()
        {
            return CurrentState == CardState.Hovered;
        }

        public bool CanTransitionToReturningToHand()
        {
            return CurrentState == CardState.Dragged;
        }

        public bool CanTransitionToNone()
        {
            return CurrentState == CardState.Hovered
                || CurrentState == CardState.ReturningToHand;
        }

        public bool TryTransitionToHovered(Action onTransition)
        {
            return TryTransition(CardState.Hovered, CanTransitionToHovered, onTransition);
        }

        public bool TryTransitionToDragged(Action onTransition)
        {
            return TryTransition(CardState.Dragged, CanTransitionToDragged, onTransition);
        }

        public bool TryTransitionToClicked(Action onTransition)
        {
            return TryTransition(CardState.Clicked, CanTransitionToClicked, onTransition);
        }

        public bool TryTransitionToReturningToHand(Action onTransition)
        {
            return TryTransition(CardState.ReturningToHand, CanTransitionToReturningToHand, onTransition);
        }

        public bool TryTransitionToNone(Action onTransition)
        {
            return TryTransition(CardState.None, CanTransitionToNone, onTransition);
        }

        public void ForceTransition(CardState targetState, Action onTransition = null)
        {
            CurrentState = targetState;
            onTransition?.Invoke();
        }

        private bool TryTransition(CardState targetState, Func<bool> guard, Action onTransition)
        {
            if (!guard())
            {
                return false;
            }

            CurrentState = targetState;
            onTransition?.Invoke();

            return true;
        }
    }
}