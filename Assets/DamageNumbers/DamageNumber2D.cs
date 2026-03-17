using Assets.Cards.Constants;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace Assets.DamageNumbers
{
    public readonly struct DamageNumber2DConfig
    {
        public int Damage { get; }
        public DamageNumberAppearance Appearance { get; }

        public DamageNumber2DConfig(int damage, DamageNumberAppearance appearance)
        {
            Damage = damage;
            Appearance = appearance;
        }
    }

    public class DamageNumber2D : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event EventHandler OnLifeEnd;

        private Sequence _lifeSequence;

        private void OnDisable()
        {
            _lifeSequence?.Kill();
            _lifeSequence = null;
        }

        public void Initialize(DamageNumber2DConfig config)
        {
            _lifeSequence?.Kill();
            SetTextAppearance(config);

            float startFontSize = _textMeshPro.fontSize;
            float targetFontSize = startFontSize * config.Appearance.GrowFontSizeAnimationScaleMultiplier;

            _lifeSequence = DOTween.Sequence();
            _lifeSequence
                .Append(DOTween.To(() => _textMeshPro.fontSize, v => _textMeshPro.fontSize = v, targetFontSize, DamageNumberConstants.RESIZE_ANIMATION_SPEED)
                    .SetEase(Ease.InOutSine))
                .Join(_canvasGroup.DOFade(1f, 0.05f))
                .Append(DOTween.To(() => _textMeshPro.fontSize, v => _textMeshPro.fontSize = v, 0f, DamageNumberConstants.RESIZE_ANIMATION_SPEED)
                    .SetEase(Ease.InOutSine))
                .Join(_canvasGroup.DOFade(0f, DamageNumberConstants.FADE_ANIMATION_SPEED))
                .OnComplete(() => OnLifeEnd?.Invoke(this, EventArgs.Empty));
        }

        private void SetTextAppearance(DamageNumber2DConfig config)
        {
            _textMeshPro.text = config.Damage.ToString();
            _textMeshPro.color = config.Appearance.Color;
            _textMeshPro.fontSize = config.Appearance.FontSize;
            _canvasGroup.alpha = 1f;
        }
    }
}
