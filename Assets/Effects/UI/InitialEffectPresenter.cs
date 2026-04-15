using Assets.Effects.Base;
using Assets.Interfaces;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Effects.UI
{
    public struct InitialEffectPresenterConfig
    {
        public AnimatorController InitialEffectAnimator;

        public InitialEffectPresenterConfig(AnimatorController initialEffectAnimator)
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

            string defaultStatePath = GetDefaultStatePath(config.InitialEffectAnimator);
            if (!string.IsNullOrEmpty(defaultStatePath))
            {
                _animator.Play(defaultStatePath, 0, 0f);
            }

            _animator.Update(0f);

            float animationDuration = GetDefaultStateDuration(config.InitialEffectAnimator);
            if (animationDuration <= 0f)
            {
                animationDuration = GetAnimationDuration(config.InitialEffectAnimator);
            }

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

        private static string GetDefaultStatePath(AnimatorController animatorController)
        {
            if (animatorController.layers == null || animatorController.layers.Length == 0)
            {
                return null;
            }

            var layer = animatorController.layers[0];
            var defaultState = layer.stateMachine?.defaultState;
            if (defaultState == null)
            {
                return null;
            }

            return layer.name + "." + defaultState.name;
        }

        private static float GetDefaultStateDuration(AnimatorController animatorController)
        {
            if (animatorController.layers == null || animatorController.layers.Length == 0)
            {
                return 0f;
            }

            var defaultState = animatorController.layers[0].stateMachine?.defaultState;
            if (defaultState == null)
            {
                return 0f;
            }

            if (defaultState.motion is AnimationClip animationClip)
            {
                return animationClip.length;
            }

            return 0f;
        }

        private static float GetAnimationDuration(AnimatorController animatorController)
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