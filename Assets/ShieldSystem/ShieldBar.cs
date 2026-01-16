using Assets.TweenCustom;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.ShieldSystem
{
    public class ShieldBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private GameObject _barContainer;
        [SerializeField] private ShieldReceiver _shield;
        [SerializeField] private TextMeshProUGUI _shieldText;

        private int _maxShieldDisplayed;

        private void Awake()
        {
            HideBar();
        }

        private void Start()
        {
            _shield.OnShieldChanged += Shield_OnShieldChanged;
            _shield.OnShieldDepleted += Shield_OnShieldDepleted;
            _shield.OnShieldGained += Shield_OnShieldGained;

            UpdateBar();
        }

        private void OnDestroy()
        {
            _shield.OnShieldChanged -= Shield_OnShieldChanged;
            _shield.OnShieldDepleted -= Shield_OnShieldDepleted;
            _shield.OnShieldGained -= Shield_OnShieldGained;
        }

        private void Shield_OnShieldGained(object sender, EventArgs e)
        {
            _maxShieldDisplayed = _shield.CurrentShield;

            _slider.maxValue = _maxShieldDisplayed;

            ShowBar();

            UpdateBar();
        }

        private void Shield_OnShieldChanged(object sender, EventArgs e)
        {
            if (_shield.CurrentShield > _maxShieldDisplayed)
            {
                _maxShieldDisplayed = _shield.CurrentShield;
            }

            UIShakeEffects.WeakShake(transform);

            UpdateBar();
        }

        private void Shield_OnShieldDepleted(object sender, EventArgs e)
        {
            HideBar();

            _maxShieldDisplayed = 0;
        }

        private void UpdateBar()
        {
            if (_maxShieldDisplayed > 0)
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
