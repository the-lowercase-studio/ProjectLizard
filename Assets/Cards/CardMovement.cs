using UnityEngine;

namespace Assets.Cards
{
    public interface ICardMovement
    {
        Vector3 GetPosition();

        void SetPosition(Vector3 pos);

        void SetVisualPosition(Vector3 pos);

        Vector3 GetVisualPosition();
    }

    [RequireComponent(typeof(Card))]
    public class CardMovement : MonoBehaviour, ICardMovement
    {
        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void SetPosition(Vector3 pos)
        {
            _card.transform.position = pos;
        }

        public void SetVisualPosition(Vector3 pos)
        {
            _card.Visual.transform.position = pos;
        }

        public Vector3 GetPosition()
        {
            return _card.transform.position;
        }

        public Vector3 GetVisualPosition()
        {
            return _card.Visual.transform.position;
        }
    }
}
