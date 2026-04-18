using Assets.Cards.Constants;
using Assets.TweenCustom;
using DG.Tweening;
using UnityEngine;

namespace Assets.Cards.Base
{
    public interface ICardScaler : ITweenUser
    {
        void SetVisualScale(Vector3 scale, bool withTweening = true);

        void ScaleVisualUp(float scaleFactor = CardConstants.Scaling.DEFAULT_SCALING_FACTOR,
            bool withTweening = true);

        void ResetVisualScale(bool withTweening = true);

        Vector3 GetVisualScale();

        void SetVisualPivotToBottom();

        void RestoreVisualPivotToCenter();

        void ScaleVisualUpFromBottom(float scaleFactor = CardConstants.Scaling.DEFAULT_SCALING_FACTOR,
            bool withTweening = true);

        void ResetVisualScaleFromBottom(bool withTweening = true);
    }

    [RequireComponent(typeof(Card))]
    public class CardScaler : MonoBehaviour, ICardScaler
    {
        private Card _card;
        private Tween _visualScaleTween;
        private Vector3 _originalScale = Vector3.one;
        private Vector2 _originalPivot = new Vector2(0.5f, 0.5f);
        private RectTransform _visualRectTransform;

        private void Awake()
        {
            _card = GetComponent<Card>();
            _visualRectTransform = _card.Visual.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _originalScale = _card.Visual.transform.localScale;
            _originalPivot = _visualRectTransform.pivot;
        }

        public void SetVisualScale(Vector3 scale, bool withTweening = true)
        {
            if (_card.Visual == null)
            {
                return;
            }

            if (withTweening)
            {
                _visualScaleTween.KillIfPlaying();

                _visualScaleTween = _card.Visual.transform
                    .DOScale(scale, CardConstants.Scaling.SCALING_DURATION)
                    .SetEase(Ease.OutBack);
            }
            else
            {
                _card.Visual.transform.localScale = scale;
            }
        }

        public void ScaleVisualUp(float scaleFactor = CardConstants.Scaling.DEFAULT_SCALING_FACTOR, bool withTweening = true)
        {
            Vector3 targetScale = _originalScale * scaleFactor;
            SetVisualScale(targetScale, withTweening);
        }

        public void ResetVisualScale(bool withTweening = true)
        {
            SetVisualScale(_originalScale, withTweening);
        }

        public Vector3 GetVisualScale()
        {
            if (_card.Visual != null)
            {
                return _card.Visual.transform.localScale;
            }

            return Vector3.one;
        }

        public void SetVisualPivotToBottom()
        {
            SetPivotWithOffsetCompensation(new Vector2(0.5f, 0f));
        }

        public void RestoreVisualPivotToCenter()
        {
            SetPivotWithOffsetCompensation(_originalPivot);
        }

        public void ScaleVisualUpFromBottom(float scaleFactor = CardConstants.Scaling.DEFAULT_SCALING_FACTOR, bool withTweening = true)
        {
            SetVisualPivotToBottom();
            ScaleVisualUp(scaleFactor, withTweening);
        }

        public void ResetVisualScaleFromBottom(bool withTweening = true)
        {
            ResetVisualScale(withTweening);
            RestoreVisualPivotToCenter();
        }

        public void StopTweens()
        {
            _visualScaleTween.KillIfPlaying();
        }

        private void SetPivotWithOffsetCompensation(Vector2 targetPivot)
        {
            if (_visualRectTransform == null)
            {
                return;
            }

            Vector2 currentPivot = _visualRectTransform.pivot;
            if (currentPivot == targetPivot)
            {
                return;
            }

            Vector2 currentPosition = _visualRectTransform.anchoredPosition;
            Rect rect = _visualRectTransform.rect;
            Vector3 currentScale = _visualRectTransform.localScale;

            Vector2 pivotDelta = targetPivot - currentPivot;
            Vector2 positionOffset = new Vector2(
                pivotDelta.x * rect.width * currentScale.x,
                pivotDelta.y * rect.height * currentScale.y
            );

            _visualRectTransform.pivot = targetPivot;
            _visualRectTransform.anchoredPosition = currentPosition + positionOffset;
        }
    }
}
