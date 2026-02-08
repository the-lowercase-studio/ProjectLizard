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
    }

    [RequireComponent(typeof(Card))]
    public class CardScaler : MonoBehaviour, ICardScaler
    {
        private Card _card;
        private Tween _visualScaleTween;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        private void OnEnable()
        {
            _originalScale = _card.Visual.transform.localScale;
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

        public void StopTweens()
        {
            _visualScaleTween.KillIfPlaying();
        }
    }
}
