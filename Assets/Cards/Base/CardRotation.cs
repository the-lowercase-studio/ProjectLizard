using DG.Tweening;
using UnityEngine;

namespace Assets.Cards.Base
{
    public interface ICardRotation
    {
        void SetZRotation(float rotation, bool withTweening = false);

        void SetZVisualRotation(float rotation, bool withTweening = false);

        Vector3 GetEulerRotation();

        Vector3 GetVisualEulerRotation();
    }

    [RequireComponent(typeof(Card))]
    public class CardRotation : MonoBehaviour, ICardRotation
    {
        private const float ROTATION_TWEEN_DURATION = 0.4f;
        private Card _card;
        private Tween _visualRotationTween;

        private void Awake()
        {
            _card = GetComponent<Card>();
        }

        public void SetZRotation(float rotation, bool withTweening = false)
        {
            SetRotation(transform, rotation, withTweening);
        }

        public void SetZVisualRotation(float rotation, bool withTweening = false)
        {
            SetRotation(_card.Visual.transform, rotation, withTweening);
        }

        public Vector3 GetVisualEulerRotation()
        {
            return _card.transform.rotation.eulerAngles;
        }

        public Vector3 GetEulerRotation()
        {
            return _card.Visual.transform.rotation.eulerAngles;
        }

        private void SetRotation(Transform transform, float rotation, bool withTweening = false)
        {
            Vector3 newRotation = new Vector3(transform.rotation.x, transform.rotation.y, rotation);

            if (withTweening)
            {
                if (_visualRotationTween?.IsPlaying() == true)
                {
                    _visualRotationTween.Kill();
                }

                _visualRotationTween = transform
                    .DORotate(newRotation, ROTATION_TWEEN_DURATION, RotateMode.Fast)
                    .SetEase(Ease.OutSine);
            }
            else
            {
                transform.rotation = Quaternion.Euler(newRotation);
            }
        }
    }
}