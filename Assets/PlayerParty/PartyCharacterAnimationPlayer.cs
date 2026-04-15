using UnityEngine;

public interface IPartyCharacterAnimationPlayer
{
    bool TryPlayAttack();

    void SetAnimatorController(RuntimeAnimatorController animatorController);
}

public class PartyCharacterAnimationPlayer : MonoBehaviour, IPartyCharacterAnimationPlayer
{
    private const string ATTACK_TRIGGER = "Attack";

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    public bool TryPlayAttack()
    {
        return TrySetTrigger(ATTACK_TRIGGER);
    }

    public void SetAnimatorController(RuntimeAnimatorController animatorController)
    {
        if (_animator == null || animatorController == null)
        {
            return;
        }

        _animator.runtimeAnimatorController = animatorController;
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