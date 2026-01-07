using System;
using UnityEngine;

namespace Assets.Energy
{
    public sealed class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

        private const byte START_ENERGY_PER_TURN = 3;
        private const byte MAX_ENERGY_PER_TURN = 9;

        private int _energyPerTurn = START_ENERGY_PER_TURN;
        private int _currentEnergy = START_ENERGY_PER_TURN;

        public event EventHandler<int> OnCurrentEnergyChange;

        public event EventHandler<int> OnEnergyPerTurnChange;

        public int EnergyPerTurn => _energyPerTurn;
        public int CurrentEnergy => _currentEnergy;

        private EnergyManager()
        { }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RefilCurrentEnergy()
        {
            _currentEnergy = _energyPerTurn;
        }

        public void DecreaseCurrentEnergy(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int diff = CurrentEnergy - amount;
            if (diff >= 0)
            {
                _currentEnergy = diff;

                OnCurrentEnergyChange?.Invoke(this, _currentEnergy);
            }
        }

        public void IncreaseCurrentEnergy(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int sum = CurrentEnergy + amount;
            if (sum <= _energyPerTurn)
            {
                _currentEnergy = sum;

                OnCurrentEnergyChange?.Invoke(this, _currentEnergy);
            }
        }

        public void DecreaseEnergyPerTurn(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int diff = EnergyPerTurn - amount;
            if (diff >= START_ENERGY_PER_TURN)
            {
                _energyPerTurn = diff;

                OnEnergyPerTurnChange?.Invoke(this, _energyPerTurn);
            }
        }

        public void IncreaseEnergyPerTurn(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int sum = EnergyPerTurn + amount;
            if (sum <= MAX_ENERGY_PER_TURN)
            {
                _energyPerTurn = sum;

                OnEnergyPerTurnChange?.Invoke(this, _energyPerTurn);
            }
        }
    }
}
