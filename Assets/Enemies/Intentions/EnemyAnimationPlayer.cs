using Assets.Enemies.Base;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Enemies.Intentions
{
    public interface IEnemyAnimationPlayer
    {
        void TryPlayForIntention(IntentionType intentionType);

        void PlayDeath(Action onCompleted);
    }

    public class EnemyAnimationPlayer : MonoBehaviour, IEnemyAnimationPlayer
    {
        private const string ATTACK_TRIGGER = "Attack";
        private const string DEFENSE_TRIGGER = "Defense";
        private const string SPECIAL_TRIGGER = "Special";
        private const string SELF_PARALYSIS_TRIGGER = "SelfParalysis";
        private const string DIE_TRIGGER = "Die";
        private const int BASE_ANIMATION_LAYER = 0;

        [SerializeField] private Animator _animator;

        [Header("Death Animation")]
        [SerializeField][Min(0f)] private float _deathAnimationFallbackDuration = 0.35f;

        private IEnemyBase _enemy;
        private Coroutine _deathAnimationCoroutine;

        private void Awake()
        {
            _enemy = GetComponentInParent<IEnemyBase>();

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        public void TryPlayForIntention(IntentionType intentionType)
        {
            if (!IsIntentionHandledByEnemy(intentionType))
            {
                return;
            }

            string triggerName = GetTriggerNameForIntention(intentionType);
            TrySetTrigger(triggerName);
        }

        public void PlayDeath(Action onCompleted)
        {
            if (_deathAnimationCoroutine != null)
            {
                StopCoroutine(_deathAnimationCoroutine);
                _deathAnimationCoroutine = null;
            }

            if (!TrySetTrigger(DIE_TRIGGER))
            {
                onCompleted?.Invoke();
                return;
            }

            _deathAnimationCoroutine = StartCoroutine(WaitForDeathAnimationAndComplete(onCompleted));
        }

        private IEnumerator WaitForDeathAnimationAndComplete(Action onCompleted)
        {
            // Allow one frame so animator state has time to transition after trigger is set.
            yield return null;

            float waitDuration = _deathAnimationFallbackDuration;
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(BASE_ANIMATION_LAYER);
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(BASE_ANIMATION_LAYER);

            if (_animator.IsInTransition(BASE_ANIMATION_LAYER) && nextState.length > 0f)
            {
                waitDuration = Mathf.Max(waitDuration, nextState.length);
            }
            else if (currentState.length > 0f)
            {
                waitDuration = Mathf.Max(waitDuration, currentState.length);
            }

            if (waitDuration > 0f)
            {
                yield return new WaitForSeconds(waitDuration);
            }

            _deathAnimationCoroutine = null;
            onCompleted?.Invoke();
        }

        private string GetTriggerNameForIntention(IntentionType intentionType)
        {
            switch (intentionType)
            {
                case IntentionType.Attack:
                    return ATTACK_TRIGGER;

                case IntentionType.Defense:
                    return DEFENSE_TRIGGER;

                case IntentionType.Special:
                    return SPECIAL_TRIGGER;

                case IntentionType.SelfParalysis:
                    return SELF_PARALYSIS_TRIGGER;

                default:
                    return string.Empty;
            }
        }

        private bool IsIntentionHandledByEnemy(IntentionType intentionType)
        {
            if (_enemy?.Config?.Intentions == null)
            {
                return false;
            }

            foreach (IntentionConfig intention in _enemy.Config.Intentions)
            {
                if (intention != null && intention.IntentionType == intentionType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TrySetTrigger(string triggerName)
        {
            if (_animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            if (!HasTrigger(triggerName))
            {
                return false;
            }

            _animator.ResetTrigger(triggerName);
            _animator.SetTrigger(triggerName);
            return true;
        }

        private bool HasTrigger(string triggerName)
        {
            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}