using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Effects.UI
{
    public struct EffectPresenterConfig
    {
        public IStatusEffect Effect;
        public Sprite EffectSprite;

        public EffectPresenterConfig(IStatusEffect effect, Sprite effectSprite)
        {
            Effect = effect;
            EffectSprite = effectSprite;
        }
    }

    public interface IEffectPresenter : IInitializableByConfig<EffectPresenterConfig>
    {
        string GetEffectName();

        IStatusEffect GetTrackedEffect();

        bool IsEffectExpired();

        void UpdateDisplay();
    }

    public class EffectPresenter : MonoBehaviour, IEffectPresenter
    {
        [SerializeField] private Image _effectIcon;
        [SerializeField] private GameObject _counterContainer;
        [SerializeField] private TextMeshProUGUI _turnCounterText;
        [SerializeField] private TextMeshProUGUI _effectValueText;

        private IStatusEffect _trackedEffect;

        public void Initialize(EffectPresenterConfig config)
        {
            _trackedEffect = config.Effect;
            _effectIcon.sprite = config.EffectSprite;

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_trackedEffect == null)
            {
                return;
            }

            int remainingTurns = _trackedEffect.RemainingTurns;
            _turnCounterText.text = remainingTurns.ToString();
            _counterContainer.SetActive(remainingTurns > 0);

            if (!string.IsNullOrEmpty(_trackedEffect.EffectValueDisplay) && _trackedEffect.EffectValueDisplay != "0")
            {
                _effectValueText.text = _trackedEffect.EffectValueDisplay;
                _effectValueText.gameObject.SetActive(true);
            }
            else
            {
                _effectValueText.gameObject.SetActive(false);
            }
        }

        public bool IsEffectExpired()
        {
            return _trackedEffect == null || _trackedEffect.RemainingTurns <= 0;
        }

        public IStatusEffect GetTrackedEffect()
        {
            return _trackedEffect;
        }

        public string GetEffectName()
        {
            return _trackedEffect?.EffectType.ToString();
        }
    }
}
