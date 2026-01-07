using TMPro;
using UnityEngine;

namespace Assets.Energy
{
    public class EnergyPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _energyText;
        private EnergyManager _energyManager;

        private void Start()
        {
            _energyManager = EnergyManager.Instance;

            _energyManager.OnCurrentEnergyChange += EnergyManager_OnCurrentEnergyChange;
            _energyManager.OnEnergyPerTurnChange += EnergyManager_OnEnergyPerTurnChange;

            UpdateCurrentEnergyText(_energyManager.CurrentEnergy);
            UpdateEnergyPerTurnText(_energyManager.EnergyPerTurn);
        }

        private void OnDisable()
        {
            _energyManager.OnCurrentEnergyChange -= EnergyManager_OnCurrentEnergyChange;
            _energyManager.OnEnergyPerTurnChange -= EnergyManager_OnEnergyPerTurnChange;
        }

        private void EnergyManager_OnCurrentEnergyChange(object sender, int value)
        {
            UpdateCurrentEnergyText(value);
        }

        private void EnergyManager_OnEnergyPerTurnChange(object sender, int value)
        {
            UpdateEnergyPerTurnText(value);
        }

        private void UpdateCurrentEnergyText(int value)
        {
            _energyText.text = $"{value}/{_energyText.text.Split('/')[1]}";
        }

        private void UpdateEnergyPerTurnText(int value)
        {
            _energyText.text = $"{_energyText.text.Split('/')[0]}/{value}";
        }
    }
}
