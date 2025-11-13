using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector3 GetRectAnchoredPosition();

        Vector3 GetVisualRectAnchoredPosition();

        void SetVisualRectAnchoredPosition(Vector3 pos);
    }

    [RequireComponent(typeof(Card))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        private Card _card;
        private RectTransform _rectTransform;
        private RectTransform _visualRectTransform;

        private void Awake()
        {
            _card = GetComponent<Card>();

            _rectTransform = _card.gameObject.GetComponent<RectTransform>();

            _visualRectTransform = _card.Visual.gameObject.GetComponent<RectTransform>();
        }

        public Vector3 GetRectAnchoredPosition()
        {
            if (_rectTransform != null)
            {
                return _rectTransform.anchoredPosition;
            }

            return Vector3.zero;
        }

        public Vector3 GetVisualRectAnchoredPosition()
        {
            if (_visualRectTransform != null)
            {
                return _visualRectTransform.anchoredPosition;
            }

            return Vector3.zero;
        }

        public void SetVisualRectAnchoredPosition(Vector3 pos)
        {
            _visualRectTransform.anchoredPosition = pos;
        }
    }
}
