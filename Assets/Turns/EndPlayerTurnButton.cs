using Reflex.Attributes;
using UnityEngine;

namespace Assets.Turns
{
    public class EndPlayerTurnButton : MonoBehaviour
    {
        [Inject] private ITurnManager _turnManager;

        [SerializeField] private GameObject _button;

        private void Start()
        {
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
