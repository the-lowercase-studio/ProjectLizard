using Assets.Inputs;
using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector2 GetRectAnchoredPosition();

        Vector2 GetVisualRectAnchoredPosition();

        void MoveCardUp();

        void SetVisualRectAnchoredPosition(Vector2 pos);

        void VisualStartFollowingPointer();

        void VisualStopFollowingPointer();
    }

    [RequireComponent(typeof(ICard))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        [SerializeField] private float hoveredCardYOffset = 20f;
        private Card _card;
        private RectTransform _rectTransform;
        private RectTransform _visualRectTransform;
        private bool _isFolowingPointer;

        private void Awake()
        {
            _card = GetComponent<Card>();

            _rectTransform = _card.gameObject.GetComponent<RectTransform>();

            _visualRectTransform = _card.Visual.gameObject.GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            if (_isFolowingPointer)
            {
                _card.Visual.position = Pointer.WorldPosition;
            }
        }

        public void VisualStartFollowingPointer()
        {
            _isFolowingPointer = true;
        }

        public void VisualStopFollowingPointer()
        {
            _isFolowingPointer = false;
        }

        public Vector2 GetRectAnchoredPosition()
        {
            if (_rectTransform != null)
            {
                return _rectTransform.anchoredPosition;
            }

            return Vector2.zero;
        }

        public Vector2 GetVisualRectAnchoredPosition()
        {
            if (_visualRectTransform != null)
            {
                return _visualRectTransform.anchoredPosition;
            }

            return Vector2.zero;
        }

        public void MoveCardUp()
        {
            var pos = GetVisualRectAnchoredPosition();

            SetVisualRectAnchoredPosition(new Vector2(
                    pos.x,
                    pos.y + hoveredCardYOffset
                )
            );
        }

        public void SetVisualRectAnchoredPosition(Vector2 pos)
        {
            _visualRectTransform.anchoredPosition = pos;
        }
    }
}
