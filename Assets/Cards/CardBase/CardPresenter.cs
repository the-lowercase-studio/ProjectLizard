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

        [SerializeField] private Image _titleBackground;
        [SerializeField] private Image _descriptionBackground;
        [SerializeField] private Image _background;
        [SerializeField] private Image _frontGraphic;

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

            _titleBackground.sprite = _card.Config.ElementalVisualBase.TitleBackground;
            _descriptionBackground.sprite = _card.Config.ElementalVisualBase.DescriptionBackground;
            _background.sprite = _card.Config.ElementalVisualBase.Background;
            _frontGraphic.sprite = _card.Config.FrontGraphic;
        }
    }
}
