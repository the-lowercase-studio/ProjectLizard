using Assets.Audio;
using Assets.Effects.UI;
using Assets.Enemies.Base;
using Assets.Enemies.Intentions;
using Assets.Enemies.UI;
using Assets.Scripts.DeathHandlers;
using Assets.Scripts.HealthSystem;
using UnityEngine;

namespace Assets.Scripts.Enemies
{
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyDeathHandler : DeathHandlerBase
    {
        private IEnemyBase _enemy;
        private IEnemyAnimationPlayer _enemyIntentAnimationPlayer;
        private IntentionIndicator _intentionIndicator;
        private HealthBar _healthBar;
        private EffectsPresenter _effectsPresenter;
        private bool _isDeathHandled;

        protected override IHealth Health => _enemy.Health;
        protected override IAudioClipPlayer AudioClipPlayer => _enemy.AudioClipPlayer;

        private void Awake()
        {
            _enemy = GetComponent<IEnemyBase>();
            _enemyIntentAnimationPlayer = GetComponentInChildren<IEnemyAnimationPlayer>();
            _intentionIndicator = GetComponentInChildren<IntentionIndicator>(true);
            _healthBar = GetComponentInChildren<HealthBar>(true);
            _effectsPresenter = GetComponentInChildren<EffectsPresenter>(true);
        }

        protected override void HandleDeath()
        {
            if (_isDeathHandled)
            {
                return;
            }

            _isDeathHandled = true;
            HideEnemyUiOnDeath();

            if (_enemyIntentAnimationPlayer != null)
            {
                _enemyIntentAnimationPlayer.PlayDeath(OnDeathAnimationFinished);
                return;
            }

            OnDeathAnimationFinished();
        }

        private void OnDeathAnimationFinished()
        {
            _enemy.Destroy();
        }

        private void HideEnemyUiOnDeath()
        {
            if (_intentionIndicator != null)
            {
                _intentionIndicator.gameObject.SetActive(false);
            }

            if (_healthBar != null)
            {
                _healthBar.gameObject.SetActive(false);
            }

            if (_effectsPresenter != null)
            {
                _effectsPresenter.gameObject.SetActive(false);
            }
        }
    }
}
