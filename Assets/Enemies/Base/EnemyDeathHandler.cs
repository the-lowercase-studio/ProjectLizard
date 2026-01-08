using Assets.Interfaces;
using Assets.VFX;
using System;
using UnityEngine;

namespace Assets.Scripts.Enemies
{
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyDeathHandler : MonoBehaviour, INeedToCompleteBeforeDisable
    {
        [SerializeField] private GameObject _visual;
        [SerializeField] private VFXPlayer _deathVfxPlayer;
        private EnemyBase _enemy;
        private byte _startEffectsToFinish = 2;
        private byte _effectsToFinish;

        public event EventHandler OnCompleted;

        private void Awake()
        {
            _enemy = GetComponent<EnemyBase>();
        }

        private void OnEnable()
        {
            _effectsToFinish = _startEffectsToFinish;

            _deathVfxPlayer.OnVFXFinished += OnDeathEffectFinishedPlaying;
            _enemy.AudioClipPlayer.OnAudioClipFinished += OnDeathEffectFinishedPlaying;

            _enemy.Health.OnNoHealth += Health_OnNoHealth;
        }

        private void OnDisable()
        {
            _deathVfxPlayer.OnVFXFinished -= OnDeathEffectFinishedPlaying;
            _enemy.AudioClipPlayer.OnAudioClipFinished -= OnDeathEffectFinishedPlaying;

            _enemy.Health.OnNoHealth -= Health_OnNoHealth;
        }

        private void Health_OnNoHealth(object sender, EventArgs e)
        {
            _visual.SetActive(false);

            _deathVfxPlayer.Play(new VFXPlayConfig());

            _enemy.AudioClipPlayer.Play("Death");
        }

        private void OnDeathEffectFinishedPlaying(object sender, EventArgs e)
        {
            _effectsToFinish--;

            if (_effectsToFinish == 0)
            {
                OnCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
