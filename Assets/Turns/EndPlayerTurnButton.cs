using UnityEngine;

namespace Assets.Turns
{
    public class EndPlayerTurnButton : MonoBehaviour
    {
        [SerializeField] private GameObject _button;
        private TurnManager _turnManager;

        private void Start()
        {
            _turnManager = TurnManager.Instance;
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
            _turnManager.OnPlayerTurnEnd += TurnManager_OnPlayerTurnEnd;
        }

        private void OnDisable()
        {
            _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
            _turnManager.OnPlayerTurnEnd -= TurnManager_OnPlayerTurnEnd;
        }

        private void TurnManager_OnPlayerTurnStart(object sender, System.EventArgs e)
        {
            _button.SetActive(true);
        }

        private void TurnManager_OnPlayerTurnEnd(object sender, System.EventArgs e)
        {
            _button.SetActive(false);
        }

        public void OnClick()
        {
            _turnManager.EndPlayerTurn();
        }
    }
}
