using Assets.Enemies.Intentions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Enemies.UI
{
    public class IntentionIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private Image _intentionIcon;

        [Header("Intention Icons")]
        [SerializeField] private Sprite _attackIcon;
        [SerializeField] private Sprite _defenseIcon;
        [SerializeField] private Sprite _specialIcon;
        [SerializeField] private Sprite _selfParalysisIcon;

        public void ShowActionIntention(IntentionConfig intention)
        {
            gameObject.SetActive(true);

            switch (intention.IntentionType)
            {
                case IntentionType.Attack:
                    _intentionIcon.sprite = _attackIcon;
                    break;

                case IntentionType.Defense:
                    _intentionIcon.sprite = _defenseIcon;
                    break;

                case IntentionType.Special:
                    _intentionIcon.sprite = _specialIcon;
                    break;

                case IntentionType.SelfParalysis:
                    _intentionIcon.sprite = _selfParalysisIcon;
                    SetValueTextFromIntention();
                    return;

                default:
                    _intentionIcon.sprite = null;
                    break;
            }

            SetValueTextFromIntention(intention);
        }

        private void SetValueTextFromIntention(IntentionConfig intention = null)
        {
            if (intention?.Action != null)
            {
                int value = intention.Action.GetValue();
                _valueText.text = value > 0 ? value.ToString() : string.Empty;
            }
            else
            {
                _valueText.text = string.Empty;
            }
        }
    }
}
