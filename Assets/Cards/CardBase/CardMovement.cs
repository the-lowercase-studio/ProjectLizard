using Assets.Constants;
using Assets.Inputs;
using Assets.TweenCustom;
using DG.Tweening;
using System;
using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector2 GetRectAnchoredPosition();

        Vector2 GetVisualRectAnchoredPosition();

        void MoveCardUp(Action callback = null);

        void SetVisualRectAnchoredPosition(Vector2 pos, TweenConfig config);

        void VisualStartFollowingPointer();

        void VisualStopFollowingPointer();
    }

    [RequireComponent(typeof(ICard))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        [SerializeField] private float hoveredCardYOffset = 20f;
        private Card _card;
        private RectTransform _rectTransform;
        private RectTransform _visualRectTransform;
        private bool _isFolowingPointer;
        private Tween _visualMovementTween;
        private Vector2 _lastTargetPos = Vector3.zero;

        private void Awake()
        {
            _card = GetComponent<Card>();

            _rectTransform = _card.gameObject.GetComponent<RectTransform>();

            _visualRectTransform = _card.Visual.gameObject.GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            if (_isFolowingPointer
                && Vector2.Distance(_lastTargetPos, Pointer.WorldPosition) >= PositionConstants.DISTANCE_ACCURACY)
            {
                _card.Visual.position = Pointer.WorldPosition;
                _lastTargetPos = Pointer.WorldPosition;
            }
        }

        public void VisualStartFollowingPointer()
        {
            if (_visualMovementTween?.IsPlaying() == true)
            {
                _visualMovementTween.Kill();
            }

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

        public void MoveCardUp(Action callback = null)
        {
            var pos = GetVisualRectAnchoredPosition();

            SetVisualRectAnchoredPosition(new Vector2(
                    pos.x,
                    pos.y + hoveredCardYOffset
                ),
                new TweenConfig(MovementConstants.Tween.CARD_HOVER_UP_MOVEMENT_DURATION, callback)
            );
        }

        public void SetVisualRectAnchoredPosition(Vector2 pos, TweenConfig config)
        {
            if (config.Equals(default(TweenConfig)))
            {
                _visualRectTransform.anchoredPosition = pos;
            }
            else
            {
                if (_visualMovementTween?.IsPlaying() == true)
                {
                    _visualMovementTween.Kill();
                }

                _visualMovementTween = _visualRectTransform
                    .DOAnchorPos(pos, config.TweenDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => config.Callback?.Invoke());
            }
        }
    }
}
