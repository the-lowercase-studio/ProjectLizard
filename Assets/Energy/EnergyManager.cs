using UnityEngine;

namespace Assets.Energy
{
    public sealed class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

        private const byte START_ENERGY_PER_TURN = 3;
        private const byte MAX_ENERGY_PER_TURN = 9;

        private byte _energyPerTurn = START_ENERGY_PER_TURN;
        private byte _currentEnergy = START_ENERGY_PER_TURN;

        public byte EnergyPerTurn
        {
            get => _energyPerTurn;
            set
            {
                if (value == 0)
                {
                    _energyPerTurn = 1;
                }
                else if (MAX_ENERGY_PER_TURN <= value)
                {
                    _energyPerTurn = value;
                }
                else
                {
                    _energyPerTurn = MAX_ENERGY_PER_TURN;
                }
            }
        }

        public byte CurrentEnergy
        {
            get => _currentEnergy;
            set
            {
                if (MAX_ENERGY_PER_TURN <= value)
                {
                    _energyPerTurn = value;
                }
                else
                {
                    _energyPerTurn = MAX_ENERGY_PER_TURN;
                }
            }
        }

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
    }
}
