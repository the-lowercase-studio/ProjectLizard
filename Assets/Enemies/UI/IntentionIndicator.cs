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

        public void ShowIntention(IntentionConfig intention)
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
            }

            int value = intention.Action.GetValue();
            Debug.Log($"VALUE TO SHOW: {value}");
            _valueText.text = value > 0 ? value.ToString() : string.Empty;
        }
    }
}
