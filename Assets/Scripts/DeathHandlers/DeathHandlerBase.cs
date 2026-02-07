using Assets.Interfaces;
using Assets.Scripts.HealthSystem;
using Assets.Audio;
using Assets.VFX;
using System;
using UnityEngine;

namespace Assets.Scripts.DeathHandlers
{
    public abstract class DeathHandlerBase : MonoBehaviour, INeedToCompleteBeforeDisable
    {
        [SerializeField] protected VFXPlayer _deathVfxPlayer;
        private byte _startEffectsToFinish = 2;
        private byte _effectsToFinish;

        public event EventHandler OnCompleted;

        protected abstract IHealth Health { get; }
        protected abstract IAudioClipPlayer AudioClipPlayer { get; }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        protected virtual void Start()
        {
            _effectsToFinish = _startEffectsToFinish;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _deathVfxPlayer.OnVFXFinished += OnDeathEffectFinishedPlaying;
            AudioClipPlayer.OnAudioClipFinished += OnDeathEffectFinishedPlaying;
            Health.OnNoHealth += Health_OnNoHealth;
        }

        private void UnsubscribeFromEvents()
        {
            _deathVfxPlayer.OnVFXFinished -= OnDeathEffectFinishedPlaying;
            AudioClipPlayer.OnAudioClipFinished -= OnDeathEffectFinishedPlaying;
            Health.OnNoHealth -= Health_OnNoHealth;
        }

        private void Health_OnNoHealth(object sender, EventArgs e)
        {
            HideVisuals();
            _deathVfxPlayer.Play(new VFXPlayConfig());
            //AudioClipPlayer.Play("Death");
        }

        protected abstract void HideVisuals();

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
