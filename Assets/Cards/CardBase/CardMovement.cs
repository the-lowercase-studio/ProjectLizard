using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector3 GetRectAnchoredPosition();

        Vector3 GetVisualRectAnchoredPosition();

        void MoveCardUp();

        void SetVisualRectAnchoredPosition(Vector3 pos);

        void SetVisualRectAnchoredPositionToPrevPosition();
    }

    [RequireComponent(typeof(Card))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        [SerializeField] private float hoveredCardYOffset = 8f;
        private Card _card;
        private RectTransform _rectTransform;
        private RectTransform _visualRectTransform;
        private Vector3 _visualRectPrevPosition;

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

        public void MoveCardUp()
        {
            var pos = GetVisualRectAnchoredPosition();

            SetVisualRectAnchoredPosition(new Vector3(
                    pos.x,
                    pos.y + hoveredCardYOffset,
                    pos.z
                )
            );

            _visualRectPrevPosition = pos;
        }

        public void SetVisualRectAnchoredPositionToPrevPosition()
        {
            SetVisualRectAnchoredPosition(_visualRectPrevPosition);
        }

        public void SetVisualRectAnchoredPosition(Vector3 pos)
        {
            _visualRectPrevPosition = GetVisualRectAnchoredPosition();
            _visualRectTransform.anchoredPosition = pos;
        }
    }
}
