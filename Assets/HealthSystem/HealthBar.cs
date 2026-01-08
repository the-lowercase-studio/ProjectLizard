using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.HealthSystem
{
    [RequireComponent(typeof(Slider))]
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Gradient _gradient;
        [SerializeField] private Health _health;
        [SerializeField] private Image _fillImage;
        [SerializeField] private bool _shakeOnHealthDecrease;
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            _slider.maxValue = _health.MaxHealth;
            _slider.value = _health.MaxHealth;
            _health.OnHealthChange += UpdateSlider_OnHealthChange;

            if (_shakeOnHealthDecrease)
            {
                _health.OnHealthDecreased += Health_OnHealthDecreased;
            }
        }

        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateSlider_OnHealthChange;

            if (_shakeOnHealthDecrease)
            {
                _health.OnHealthDecreased -= Health_OnHealthDecreased;
            }
        }

        private void UpdateSlider_OnHealthChange(object sender, System.EventArgs e)
        {
            _fillImage.color = _gradient.Evaluate(_health.CurrentHealth / _health.MaxHealth);
            _slider.value = _health.CurrentHealth;
        }

        private void Health_OnHealthDecreased(object sender, System.EventArgs e)
        {
            const float duration = 0.1f, strength = 0.10f, randomness = 90f;
            const int vibratio = 3;
            const bool snapping = true, fadeOut = true;

            transform.DOShakePosition(duration,
                                      strength,
                                      vibratio,
                                      randomness,
                                      snapping,
                                      fadeOut,
                                      ShakeRandomnessMode.Harmonic);
        }
    }
}
