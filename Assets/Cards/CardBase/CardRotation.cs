using UnityEngine;

namespace Assets.Cards
{
    public interface ICardRotation
    {
        void SetZRotation(float rotation);

        void SetZVisualRotation(float rotation);

        Vector3 GetEulerRotation();

        Vector3 GetVisualEulerRotation();
    }

    [RequireComponent(typeof(Card))]
    public class CardRotation : MonoBehaviour, ICardRotation
    {
        private Card _card;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void SetZRotation(float rotation)
        {
            _card.transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, rotation));
        }

        public void SetZVisualRotation(float rotation)
        {
            _card.Visual.transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, rotation));
        }

        public Vector3 GetVisualEulerRotation()
        {
            return _card.transform.rotation.eulerAngles;
        }

        public Vector3 GetEulerRotation()
        {
            return _card.Visual.transform.rotation.eulerAngles;
        }
    }
}
