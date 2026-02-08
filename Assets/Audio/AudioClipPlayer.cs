using System;
using System.Linq;
using UnityEngine;

namespace Assets.Audio
{
    public interface IAudioClipPlayer
    {
        public void Play(string name);

        public void PlayOneShot(string name);

        public event EventHandler OnAudioClipFinished;
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioClipPlayer : MonoBehaviour, IAudioClipPlayer
    {
        [Serializable]
        public class AudioClipPlayerConfig
        {
            public string Name;
            public AudioClipConfig[] ClipVariants;
        }

        public event EventHandler OnAudioClipFinished;

        [SerializeField] private AudioClipPlayerConfig[] _audioClipPlayerConfigs;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(string name)
        {
            AudioClipConfig clipConfigVariant = GetRandomAudioClipVariantFromConfigByName(name);

            if (clipConfigVariant is null)
            {
                return;
            }

            PrepareAudioSourceToPlayClip(clipConfigVariant);

            if (IsInvoking(nameof(OnAudioClipPlayFinished)))
            {
                CancelInvoke(nameof(OnAudioClipPlayFinished));
            }

            _audioSource.Play();

            Invoke(nameof(OnAudioClipPlayFinished), _audioSource.clip.length);
        }

        public void PlayOneShot(string name)
        {
            AudioClipConfig clipConfigVariant = GetRandomAudioClipVariantFromConfigByName(name);

            if (clipConfigVariant is null)
            {
                return;
            }

            PrepareAudioSourceToPlayClip(clipConfigVariant);

            _audioSource.PlayOneShot(_audioSource.clip);

            Invoke(nameof(OnAudioClipPlayFinished), _audioSource.clip.length);
        }

        private AudioClipConfig GetRandomAudioClipVariantFromConfigByName(string name)
        {
            AudioClipPlayerConfig config = _audioClipPlayerConfigs.FirstOrDefault(c => c.Name == name);

            if (config is null && config.ClipVariants.Length == 0)
            {
                Debug.LogError($"AudioClipPlayer: No audio clip found for name '{name}'");
                return null;
            }

            return config.ClipVariants[UnityEngine.Random.Range(0, config.ClipVariants.Length)];
        }

        private void OnAudioClipPlayFinished()
        {
            OnAudioClipFinished?.Invoke(this, EventArgs.Empty);
        }

        private void PrepareAudioSourceToPlayClip(AudioClipConfig config)
        {
            _audioSource.clip = config.AudioClip;
            _audioSource.volume = config.Volume;
            _audioSource.pitch = config.Pitch;
            _audioSource.loop = config.Loop;
        }
    }
}
