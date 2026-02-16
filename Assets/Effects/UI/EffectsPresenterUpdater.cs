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
            _turnManager.OnPlayerTurnStart += OnTurnChanged;
            _turnManager.OnEnemyTurnStart += OnTurnChanged;
            _turnManager.OnEnemyTurnEnd += OnTurnChanged;

            UpdateEffectsDisplay();
        }

        private void OnDestroy()
        {
            _turnManager.OnPlayerTurnStart -= OnTurnChanged;
            _turnManager.OnEnemyTurnStart -= OnTurnChanged;
            _turnManager.OnEnemyTurnEnd -= OnTurnChanged;
            _effectReceiver.OnEffectsChanged -= OnEffectsChanged;
        }

        public void UpdateEffectsDisplay()
        {
            _effectsPresenter.UpdateEffectsDisplay();
        }

        private void OnEffectsChanged(object sender, System.EventArgs e)
        {
            UpdateEffectsDisplay();
        }

        private void OnTurnChanged(object sender, System.EventArgs e)
        {
            UpdateEffectsDisplay();
        }
    }
}
