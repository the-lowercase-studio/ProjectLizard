using Assets.Inputs;
using DG.Tweening;
using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector2 GetRectAnchoredPosition();

        Vector2 GetVisualRectAnchoredPosition();

        void MoveCardUp(bool withTweening = false);

        void SetVisualRectAnchoredPosition(Vector2 pos, bool withTweening = false);

        void VisualStartFollowingPointer();

        void VisualStopFollowingPointer();
    }

    [RequireComponent(typeof(ICard))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        [SerializeField] private float hoveredCardYOffset = 20f;

        private float CARD_UP_MOVEMENT_DURATION = 0.5f;

        private Card _card;
        private RectTransform _rectTransform;
        private RectTransform _visualRectTransform;
        private bool _isFolowingPointer;
        private Tween _visualMovementTween;

        private void Awake()
        {
            _card = GetComponent<Card>();

            _rectTransform = _card.gameObject.GetComponent<RectTransform>();

            _visualRectTransform = _card.Visual.gameObject.GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            if (_isFolowingPointer)
            {
                _card.Visual.position = Pointer.WorldPosition;
            }
        }

        public void VisualStartFollowingPointer()
        {
            _isFolowingPointer = true;
        }

        public void VisualStopFollowingPointer()
        {
            _isFolowingPointer = false;
        }

        public Vector2 GetRectAnchoredPosition()
        {
            if (_rectTransform != null)
            {
                return _rectTransform.anchoredPosition;
            }

            return Vector2.zero;
        }

        public Vector2 GetVisualRectAnchoredPosition()
        {
            if (_visualRectTransform != null)
            {
                return _visualRectTransform.anchoredPosition;
            }

            return Vector2.zero;
        }

        public void MoveCardUp(bool withTweening = false)
        {
            var pos = GetVisualRectAnchoredPosition();

            SetVisualRectAnchoredPosition(new Vector2(
                    pos.x,
                    pos.y + hoveredCardYOffset
                ),
                withTweening
            );
        }

        public void SetVisualRectAnchoredPosition(Vector2 pos, bool withTweening = false)
        {
            if (withTweening)
            {
                if (_visualMovementTween?.IsPlaying() == true)
                {
                    _visualMovementTween.Kill();
                }

                _visualMovementTween = _visualRectTransform
                    .DOAnchorPos(pos, CARD_UP_MOVEMENT_DURATION)
                    .SetEase(Ease.OutSine);
            }
            else
            {
                _visualRectTransform.anchoredPosition = pos;
            }
        }
    }
}
