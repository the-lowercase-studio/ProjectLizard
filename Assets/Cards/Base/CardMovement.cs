using Assets.Cards.Constants;
using Assets.Inputs.Pointer;
using Assets.TweenCustom;
using DG.Tweening;
using Reflex.Attributes;
using System;
using UnityEngine;

namespace Assets.Cards.Base
{
    public interface ICardMovement : ITweenUser
    {
        Vector2 GetRectAnchoredPosition();

        Vector2 GetVisualRectAnchoredPosition();

        void LiftCardUp(Action callback = null);

        void SetVisualRectAnchoredPosition(Vector2 pos, TweenConfig config);

        void AlignVisualWithRoot(bool withTweening = true);

        void VisualStartFollowingPointer();

        void VisualStopFollowingPointer();
    }

    [RequireComponent(typeof(ICard))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        [Inject] private IPointerPositioner _pointerPositioner;

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

            _visualRectTransform = _card.Visual.GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            if (_isFolowingPointer
                && Vector2.Distance(_lastTargetPos, _pointerPositioner.RawInputPosition) >= PositionConstants.DISTANCE_ACCURACY)
            {
                _card.Visual.transform.position = _pointerPositioner.RawInputPosition;
                _lastTargetPos = _pointerPositioner.RawInputPosition;
            }
        }

        public void VisualStartFollowingPointer()
        {
            _visualMovementTween.KillIfPlaying();

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

        public void LiftCardUp(Action callback = null)
        {
            var pos = GetVisualRectAnchoredPosition();

            SetVisualRectAnchoredPosition(new Vector2(
                    pos.x,
                    pos.y + CardConstants.Movement.HOVERED_CARD_Y_OFFSET
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
                _visualMovementTween.KillIfPlaying();

                _visualMovementTween = _visualRectTransform
                    .DOAnchorPos(pos, config.TweenDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => config.Callback?.Invoke());
            }
        }

        public void AlignVisualWithRoot(bool withTweening = true)
        {
            if (_visualRectTransform == null)
            {
                return;
            }

            float visualHeight = _visualRectTransform.rect.height;
            float yOffset = -(visualHeight / 2f) + CardConstants.Hand.Placement.Y_OFFSET;

            Vector2 targetPosition = new Vector2(0f, yOffset);

            if (withTweening)
            {
                SetVisualRectAnchoredPosition(
                    targetPosition,
                    new TweenConfig(MovementConstants.Tween.CARD_HOVER_UP_MOVEMENT_DURATION)
                );
            }
            else
            {
                SetVisualRectAnchoredPosition(targetPosition, default(TweenConfig));
            }
        }

        public void StopTweens()
        {
            _visualMovementTween.KillIfPlaying();
        }
    }
}
