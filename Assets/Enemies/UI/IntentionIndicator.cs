using Assets.Enemies.Intentions;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Enemies.UI
{
    public class IntentionIndicator : MonoBehaviour
    {
        [SerializeField] private Image _intentionIcon;

        [Header("Intention Icons")]
        [SerializeField] private Sprite _attackIcon;
        [SerializeField] private Sprite _defenseIcon;
        [SerializeField] private Sprite _specialIcon;

        public void ShowIntention(IntentionType intentionType)
        {
            gameObject.SetActive(true);

            switch (intentionType)
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
        }
    }
}
