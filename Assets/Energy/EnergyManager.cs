using Assets.Turns;
using Reflex.Attributes;
using System;
using UnityEngine;

namespace Assets.Energy
{
    public interface IEnergyManager
    {
        int EnergyPerTurn { get; }
        int CurrentEnergy { get; }

        event EventHandler<int> OnCurrentEnergyChange;

        event EventHandler<int> OnEnergyPerTurnChange;

        void RefilCurrentEnergy();

        void DecreaseCurrentEnergy(int amount);

        void IncreaseCurrentEnergy(int amount);

        void DecreaseEnergyPerTurn(int amount);

        void IncreaseEnergyPerTurn(int amount);

        void AddBonusEnergyForNextTurn(int amount);
    }

    public sealed class EnergyManager : MonoBehaviour, IEnergyManager
    {
        public int EnergyPerTurn => _activeEnergyPerTurn;
        public int CurrentEnergy => _currentEnergy;

        public event EventHandler<int> OnCurrentEnergyChange;
        public event EventHandler<int> OnEnergyPerTurnChange;

        [Inject] private ITurnManager _turnManager;

        private const byte START_ENERGY_PER_TURN = 3;
        public const byte MAX_ENERGY_PER_TURN = 9;

        private int _energyPerTurn = START_ENERGY_PER_TURN;
        private int _activeEnergyPerTurn = START_ENERGY_PER_TURN;
        private int _currentEnergy = START_ENERGY_PER_TURN;
        private int _bonusEnergyForNextTurn = 0;

        private void OnEnable()
        {
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
        }

        private void OnDisable()
        {
            _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
        }

        public void RefilCurrentEnergy()
        {
            _currentEnergy = _activeEnergyPerTurn;

            OnCurrentEnergyChange?.Invoke(this, _currentEnergy);
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
            if (sum <= _activeEnergyPerTurn)
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

            int diff = _energyPerTurn - amount;
            if (diff >= START_ENERGY_PER_TURN)
            {
                _energyPerTurn = diff;

                SetActiveEnergyPerTurn(_energyPerTurn);
            }
        }

        public void IncreaseEnergyPerTurn(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int sum = _energyPerTurn + amount;
            if (sum <= MAX_ENERGY_PER_TURN)
            {
                _energyPerTurn = sum;

                SetActiveEnergyPerTurn(_energyPerTurn);
            }
        }

        private void TurnManager_OnPlayerTurnStart(object sender, EventArgs e)
        {
            int bonusEnergyForThisTurn = _bonusEnergyForNextTurn;
            _bonusEnergyForNextTurn = 0;

            SetActiveEnergyPerTurn(Mathf.Min(_energyPerTurn + bonusEnergyForThisTurn, MAX_ENERGY_PER_TURN));
            RefilCurrentEnergy();
        }

        public void AddBonusEnergyForNextTurn(int amount)
        {
            if (amount > 0)
            {
                _bonusEnergyForNextTurn += amount;
            }
        }

        private void SetActiveEnergyPerTurn(int value)
        {
            if (_activeEnergyPerTurn == value)
            {
                return;
            }

            _activeEnergyPerTurn = value;
            if (_currentEnergy > _activeEnergyPerTurn)
            {
                _currentEnergy = _activeEnergyPerTurn;
                OnCurrentEnergyChange?.Invoke(this, _currentEnergy);
            }

            OnEnergyPerTurnChange?.Invoke(this, _activeEnergyPerTurn);
        }
    }
}
