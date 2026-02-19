using Assets.TweenCustom;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.ShieldSystem
{
    public class ShieldBar : MonoBehaviour
    {
        [SerializeField] private ShieldReceiver _shield;
        [SerializeField] private TextMeshProUGUI _shieldText;
        [SerializeField] private GameObject _barContainer;
        [SerializeField] private Slider _slider;

        private void Awake()
        {
            HideBar();
        }

        private void Start()
        {
            _shield.OnGainedShieldChanged += Shield_OnGainedShieldChanged;
            _shield.OnShieldDepleted += Shield_OnShieldDepleted;
            _shield.OnShieldGained += Shield_OnShieldGained;

            UpdateBar();
        }

        private void OnDestroy()
        {
            _shield.OnGainedShieldChanged -= Shield_OnGainedShieldChanged;
            _shield.OnShieldDepleted -= Shield_OnShieldDepleted;
            _shield.OnShieldGained -= Shield_OnShieldGained;
        }

        private void Shield_OnShieldGained(object sender, int currentShield)
        {
            _slider.maxValue = currentShield;

            ShowBar();

            UpdateBar();
        }

        private void Shield_OnGainedShieldChanged(object sender, int currentShield)
        {
            if (currentShield > _slider.maxValue)
            {
                _slider.maxValue = currentShield;
            }

            UIShakeEffects.WeakShake(transform);

            UpdateBar();
        }

        private void Shield_OnShieldDepleted(object sender, EventArgs e)
        {
            HideBar();
        }

        private void UpdateBar()
        {
            if (_slider.maxValue > 0)
            {
                _slider.value = _shield.CurrentShield;
            }
            else
            {
                _slider.value = 0;
            }

            _shieldText.text = _slider.value.ToString();
        }

        private void ShowBar()
        {
            _barContainer.SetActive(true);
        }

        private void HideBar()
        {
            _barContainer.SetActive(false);
        }
    }
}
