using System;
using UnityEngine;

namespace Assets.ShieldSystem
{
    public interface IShielded
    {
        public IShieldReceiver ShieldReceiver { get; }
    }

    public interface IShieldReceiver
    {
        int CurrentShield { get; }

        event EventHandler<int> OnGainedShieldChanged;

        event EventHandler OnShieldDepleted;

        event EventHandler<int> OnShieldGained;

        void AddShield(int amount);

        int ReduceShield(int amount);

        void ClearShield();

        bool HasShield();
    }

    [Serializable]
    public class ShieldReceiver : MonoBehaviour, IShieldReceiver
    {
        public int CurrentShield { get; private set; }

        public event EventHandler<int> OnGainedShieldChanged;
        public event EventHandler OnShieldDepleted;
        public event EventHandler<int> OnShieldGained;

        public void AddShield(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            bool hadNoShield = CurrentShield == 0;
            CurrentShield += amount;

            if (hadNoShield)
            {
                OnShieldGained?.Invoke(this, CurrentShield);
            }
            else
            {
                OnGainedShieldChanged?.Invoke(this, CurrentShield);
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

            OnGainedShieldChanged?.Invoke(this, CurrentShield);

            return remainingDamage;
        }

        public void ClearShield()
        {
            if (CurrentShield > 0)
            {
                CurrentShield = 0;
                OnShieldDepleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool HasShield()
        {
            return CurrentShield > 0;
        }
    }
}