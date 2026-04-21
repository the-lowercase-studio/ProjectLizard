using Assets.Effects.Base;
using Assets.Effects.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Cards.Base
{
    public readonly struct CardTargetEffectPreview
    {
        public EffectSO Effect { get; }
        public float Chance { get; }

        public CardTargetEffectPreview(EffectSO effect, float chance)
        {
            Effect = effect;
            Chance = Mathf.Clamp01(chance);
        }
    }

    public readonly struct CardTargetEffectChancePresenterConfig
    {
        public CardTargetEffectPreview EffectPreview { get; }

        public CardTargetEffectChancePresenterConfig(CardTargetEffectPreview effectPreview)
        {
            EffectPreview = effectPreview;
        }
    }

    public class CardTargetEffectChancePresenter : MonoBehaviour
    {
        [SerializeField] private Image _effectIcon;
        [SerializeField] private TextMeshProUGUI _chanceText;
        [SerializeField] private EffectTypeSpriteMappingSO _effectTypeMapping;

        public void Initialize(CardTargetEffectChancePresenterConfig config)
        {
            if (config.EffectPreview.Effect == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_effectIcon != null && _effectTypeMapping != null)
            {
                _effectIcon.sprite = _effectTypeMapping.GetSpriteForEffectType(config.EffectPreview.Effect.EffectType);
            }

            if (_chanceText != null)
            {
                int chancePercent = Mathf.RoundToInt(config.EffectPreview.Chance * 100f);
                _chanceText.text = chancePercent + "%";
                _chanceText.gameObject.SetActive(true);
            }
        }
    }
}
