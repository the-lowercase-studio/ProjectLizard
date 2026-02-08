using Assets.Turns;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Assets.Energy
{
    public class EnergyPresenter : MonoBehaviour
    {
        [Inject] private IEnergyManager _energyManager;
        [Inject] private ITurnManager _turnManager;

        [SerializeField] private GameObject _visual;
        [SerializeField] private TextMeshProUGUI _energyText;

        private void Start()
        {
            _energyManager.OnCurrentEnergyChange += EnergyManager_OnCurrentEnergyChange;
            _energyManager.OnEnergyPerTurnChange += EnergyManager_OnEnergyPerTurnChange;

            _turnManager.OnPlayerTurnEnd += TurnManager_OnPlayerTurnEnd;
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;

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

        private void TurnManager_OnPlayerTurnStart(object sender, System.EventArgs e)
        {
            _visual.SetActive(true);
        }

        private void TurnManager_OnPlayerTurnEnd(object sender, System.EventArgs e)
        {
            _visual.SetActive(false);
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
