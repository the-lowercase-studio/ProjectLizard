using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Combat.Shield
{
    public class ShieldBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private GameObject _barContainer;

        private IShield _shield;
        private int _maxShieldDisplayed;

        private void Awake()
        {
            HideBar();
        }

        private void OnDestroy()
        {
            UnsubscribeFromShield(_shield);
        }

        public void Initialize(IShield shield)
        {
            UnsubscribeFromShield(_shield);

            if (shield != null)
            {
                _shield = shield;

                SubscribeOnShield(_shield);

                UpdateBar();
            }
        }

        private void SubscribeOnShield(IShield shield)
        {
            if (shield != null)
            {
                shield.OnShieldChanged += Shield_OnShieldChanged;
                shield.OnShieldDepleted += Shield_OnShieldDepleted;
                shield.OnShieldGained += Shield_OnShieldGained;
            }
        }

        private void UnsubscribeFromShield(IShield shield)
        {
            if (shield != null)
            {
                shield.OnShieldChanged -= Shield_OnShieldChanged;
                shield.OnShieldDepleted -= Shield_OnShieldDepleted;
                shield.OnShieldGained -= Shield_OnShieldGained;
            }
        }

        private void Shield_OnShieldGained(object sender, EventArgs e)
        {
            _maxShieldDisplayed = _shield.CurrentShield;
            ShowBar();
            UpdateBar();
        }

        private void Shield_OnShieldChanged(object sender, EventArgs e)
        {
            if (_shield.CurrentShield > _maxShieldDisplayed)
            {
                _maxShieldDisplayed = _shield.CurrentShield;
            }

            UpdateBar();
        }

        private void Shield_OnShieldDepleted(object sender, EventArgs e)
        {
            HideBar();
            _maxShieldDisplayed = 0;
        }

        private void UpdateBar()
        {
            if (_shield == null || _fillImage == null)
            {
                return;
            }

            if (_maxShieldDisplayed > 0)
            {
                float fillAmount = (float)_shield.CurrentShield / _maxShieldDisplayed;
                _fillImage.fillAmount = fillAmount;
            }
            else
            {
                _fillImage.fillAmount = 0;
            }
        }

        private void ShowBar()
        {
            if (_barContainer != null)
            {
                _barContainer.SetActive(true);
            }
        }

        private void HideBar()
        {
            if (_barContainer != null)
            {
                _barContainer.SetActive(false);
            }
        }
    }
}
