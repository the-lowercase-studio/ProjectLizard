using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.HealthSystem
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _fillImage;
        [SerializeField] private bool _shakeOnHealthDecrease;

        private void OnEnable()
        {
            _health.OnHealthChange += UpdateVisuals_OnHealthChange;
            _health.OnHealthInitialization += Health_OnHealthInitialization;

            if (_shakeOnHealthDecrease)
            {
                _health.OnHealthDecreased += Health_OnHealthDecreased;
            }
        }

        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateVisuals_OnHealthChange;

            if (_shakeOnHealthDecrease)
            {
                _health.OnHealthDecreased -= Health_OnHealthDecreased;
            }
        }

        private void UpdateVisuals_OnHealthChange(object sender, System.EventArgs e)
        {
            _fillImage.color = _gradient.Evaluate((float)_health.CurrentHealth / _health.MaxHealth);
            _slider.value = _health.CurrentHealth;

            UpdateHealthText();
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

        private void Health_OnHealthInitialization(object sender, System.EventArgs e)
        {
            _slider.maxValue = _health.MaxHealth;
            _slider.value = _health.MaxHealth;
            UpdateHealthText();
        }

        private void UpdateHealthText()
        {
            _healthText.text = _health.CurrentHealth.ToString();
        }
    }
}
