using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Cards
{
    [RequireComponent(typeof(Card))]
    public class CardPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _cardFrontImage;

        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        private void Start()
        {
            _titleText.text = _card.Config.Title;
            _descriptionText.text = _card.Config.Description;
            _costText.text = _card.Config.StartEnergyCost.ToString();
            _cardFrontImage.sprite = _card.Config.CardFront;
        }
    }
}
