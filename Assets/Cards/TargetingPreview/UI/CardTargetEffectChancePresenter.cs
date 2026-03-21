using Assets.Effects.Base;
using Assets.Effects.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Cards.Base
{
    public readonly struct CardTargetEffectChancePresenterConfig
    {
        public EffectSO Effect { get; }

        public CardTargetEffectChancePresenterConfig(EffectSO effect)
        {
            Effect = effect;
        }
    }

    public class CardTargetEffectChancePresenter : MonoBehaviour
    {
        [SerializeField] private Image _effectIcon;
        [SerializeField] private TextMeshProUGUI _chanceText;
        [SerializeField] private EffectTypeSpriteMappingSO _effectTypeMapping;

        public void Initialize(CardTargetEffectChancePresenterConfig config)
        {
            if (config.Effect == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_effectIcon != null && _effectTypeMapping != null)
            {
                _effectIcon.sprite = _effectTypeMapping.GetSpriteForEffectType(config.Effect.EffectType);
            }

            if (_chanceText != null)
            {
                if (config.Effect is IChanceBasedEffect chanceBasedEffect)
                {
                    int chancePercent = Mathf.RoundToInt(chanceBasedEffect.ApplyChance * 100f);
                    _chanceText.text = chancePercent + "%";
                    _chanceText.gameObject.SetActive(true);
                }
                else
                {
                    _chanceText.gameObject.SetActive(false);
                }
            }
        }
    }
}
