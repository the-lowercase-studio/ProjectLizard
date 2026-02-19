using Assets.Effects.StatusEffects;
using Assets.Turns;
using UnityEngine;
using Reflex.Attributes;

namespace Assets.Effects.UI
{
    [RequireComponent(typeof(StatusEffectReceiver))]
    public class EffectsPresenterUpdater : MonoBehaviour
    {
        [Inject] private ITurnManager _turnManager;

        private IStatusEffectReceiver _effectReceiver;
        private IEffectsPresenter _effectsPresenter;

        private void Awake()
        {
            _effectReceiver = GetComponent<IStatusEffectReceiver>();
            _effectsPresenter = GetComponentInChildren<IEffectsPresenter>();
            _effectsPresenter.Initialize(_effectReceiver);
            _effectReceiver.OnEffectsChanged += OnEffectsChanged;
        }

        private void Start()
        {
            _turnManager.OnPlayerTurnStart += OnPlayerTurnStart;
            _turnManager.OnEnemyTurnStart += OnEnemyTurnStart;
            _turnManager.OnEnemyTurnEnd += OnEnemyTurnEnd;

            UpdateEffectsDisplay();
        }

        private void OnDestroy()
        {
            _turnManager.OnPlayerTurnStart -= OnPlayerTurnStart;
            _turnManager.OnEnemyTurnStart -= OnEnemyTurnStart;
            _turnManager.OnEnemyTurnEnd -= OnEnemyTurnEnd;
            _effectReceiver.OnEffectsChanged -= OnEffectsChanged;
        }

        private void OnPlayerTurnStart(object sender, System.EventArgs e)
        {
            ProcessEffectsForTurnState(TurnExecutionState.OnPlayerTurnStart);
        }

        private void OnEnemyTurnStart(object sender, System.EventArgs e)
        {
            ProcessEffectsForTurnState(TurnExecutionState.OnEnemyTurnStart);
        }

        private void OnEnemyTurnEnd(object sender, System.EventArgs e)
        {
            ProcessEffectsForTurnState(TurnExecutionState.OnEnemyTurnEnd);
        }

        private void ProcessEffectsForTurnState(TurnExecutionState turnState)
        {
            var activeEffects = _effectReceiver.GetActiveEffects();

            foreach (var effect in activeEffects)
            {
                if (effect.ExecutionState == turnState)
                {
                    effect.PerformEffect();
                }
            }

            UpdateEffectsDisplay();
        }

        private void OnEffectsChanged(object sender, System.EventArgs e)
        {
            UpdateEffectsDisplay();
        }

        public void UpdateEffectsDisplay()
        {
            _effectsPresenter.UpdateEffectsDisplay();
        }
    }
}
