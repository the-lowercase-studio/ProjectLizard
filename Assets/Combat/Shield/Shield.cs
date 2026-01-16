using System;
using UnityEngine;

namespace Assets.Combat.Shield
{
    [Serializable]
    public class Shield : MonoBehaviour, IShield
    {
        [field: SerializeField] public int CurrentShield { get; private set; }

        public event EventHandler OnShieldChanged;

        public event EventHandler OnShieldDepleted;

        public event EventHandler OnShieldGained;

        public void AddShield(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            bool hadNoShield = CurrentShield == 0;
            CurrentShield += amount;

            OnShieldChanged?.Invoke(this, EventArgs.Empty);

            if (hadNoShield)
            {
                OnShieldGained?.Invoke(this, EventArgs.Empty);
            }

            Debug.Log($"Shield added: {amount}. Current shield: {CurrentShield}");
        }

        public int ReduceShield(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            if (CurrentShield == 0)
            {
                return amount;
            }

            int remainingDamage = 0;

            if (amount >= CurrentShield)
            {
                remainingDamage = amount - CurrentShield;
                CurrentShield = 0;
                OnShieldDepleted?.Invoke(this, EventArgs.Empty);
                Debug.Log($"Shield depleted! Remaining damage: {remainingDamage}");
            }
            else
            {
                CurrentShield -= amount;
                Debug.Log($"Shield absorbed {amount} damage. Remaining shield: {CurrentShield}");
            }

            OnShieldChanged?.Invoke(this, EventArgs.Empty);

            return remainingDamage;
        }

        public void ClearShield()
        {
            if (CurrentShield > 0)
            {
                CurrentShield = 0;
                OnShieldChanged?.Invoke(this, EventArgs.Empty);
                OnShieldDepleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool HasShield()
        {
            return CurrentShield > 0;
        }
    }
}
