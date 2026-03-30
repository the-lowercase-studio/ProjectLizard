using Assets.Effects.Base;
using Assets.Interfaces;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Effects.UI
{
    public struct InitialEffectPresenterConfig
    {
        public EffectType EffectType;
        public Sprite EffectSprite;
        public AnimatorController InitialEffectAnimator;

        public InitialEffectPresenterConfig(EffectType effectType, Sprite effectSprite, AnimatorController initialEffectAnimator)
        {
            EffectType = effectType;
            EffectSprite = effectSprite;
            InitialEffectAnimator = initialEffectAnimator;
        }
    }

    public interface IInitialEffectPresenter : IInitializableByConfig<InitialEffectPresenterConfig>
    {
    }

    public class InitialEffectPresenter : MonoBehaviour, IInitialEffectPresenter
    {
        [SerializeField] private Image _effectImage;

        [SerializeField] private Animator _animator;
        private bool _isWaitingForAnimationEnd;
        private float _destroyAtTime;

        public void Initialize(InitialEffectPresenterConfig config)
        {
            if (_effectImage != null)
            {
                _effectImage.sprite = config.EffectSprite;
            }

            if (_animator == null || config.InitialEffectAnimator == null)
            {
                Destroy(gameObject);
                return;
            }

            _animator.runtimeAnimatorController = config.InitialEffectAnimator;
            _animator.Rebind();
            _animator.Update(0f);
            _animator.Play(0, 0, 0f);

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