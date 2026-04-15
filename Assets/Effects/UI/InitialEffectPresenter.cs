using Assets.Effects.Base;
using Assets.Interfaces;
using UnityEngine;

namespace Assets.Effects.UI
{
    public struct InitialEffectPresenterConfig
    {
        public RuntimeAnimatorController InitialEffectAnimator;

        public InitialEffectPresenterConfig(RuntimeAnimatorController initialEffectAnimator)
        {
            InitialEffectAnimator = initialEffectAnimator;
        }
    }

    public interface IInitialEffectPresenter : IInitializableByConfig<InitialEffectPresenterConfig>
    {
    }

    public class InitialEffectPresenter : MonoBehaviour, IInitialEffectPresenter
    {
        [SerializeField] private Animator _animator;
        private bool _isWaitingForAnimationEnd;
        private float _destroyAtTime;

        public void Initialize(InitialEffectPresenterConfig config)
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_animator == null || config.InitialEffectAnimator == null)
            {
                Destroy(gameObject);
                return;
            }

            _animator.runtimeAnimatorController = config.InitialEffectAnimator;
            _animator.Rebind();
            _animator.Update(0f);

            float animationDuration = GetAnimationDuration(config.InitialEffectAnimator);

            if (animationDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _destroyAtTime = Time.time + animationDuration;
            _isWaitingForAnimationEnd = true;
        }

        private void Update()
        {
            if (!_isWaitingForAnimationEnd)
            {
                return;
            }

            if (Time.time < _destroyAtTime)
            {
                return;
            }

            _isWaitingForAnimationEnd = false;
            Destroy(gameObject);
        }

        private static float GetAnimationDuration(RuntimeAnimatorController animatorController)
        {
            float duration = 0f;
            AnimationClip[] clips = animatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }

                if (clip.length > duration)
                {
                    duration = clip.length;
                }
            }

            return duration;
        }
    }
}