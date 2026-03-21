using Assets.Effects.Base;
using Assets.Effects.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Cards.Base
{
    public readonly struct CardTargetCrosshairPresenterConfig
    {
        public IReadOnlyList<EffectSO> Effects { get; }

        public CardTargetCrosshairPresenterConfig(IReadOnlyList<EffectSO> effects)
        {
            Effects = effects;
        }
    }

    public class CardTargetCrosshairPresenter : MonoBehaviour
    {
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private RectTransform _effectsContainer;
        [SerializeField] private CardTargetEffectChancePresenter _effectChancePresenterPrefab;

        private readonly List<CardTargetEffectChancePresenter> _activePresenters = new List<CardTargetEffectChancePresenter>();

        public void Initialize(CardTargetCrosshairPresenterConfig config)
        {
            if (_crosshairImage != null)
            {
                _crosshairImage.enabled = true;
            }

            RebuildEffects(config.Effects);
        }

        public void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            if (transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = anchoredPosition;
            }
        }

        private void OnDestroy()
        {
            ClearEffects();
        }

        private void RebuildEffects(IReadOnlyList<EffectSO> effects)
        {
            ClearEffects();

            if (_effectsContainer == null || _effectChancePresenterPrefab == null || effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                EffectSO effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                CardTargetEffectChancePresenter presenter = Instantiate(_effectChancePresenterPrefab, _effectsContainer);
                presenter.Initialize(new CardTargetEffectChancePresenterConfig(effect));
                _activePresenters.Add(presenter);
            }
        }

        private void ClearEffects()
        {
            for (int i = 0; i < _activePresenters.Count; i++)
            {
                if (_activePresenters[i] != null)
                {
                    Destroy(_activePresenters[i].gameObject);
                }
            }

            _activePresenters.Clear();
        }
    }
}
