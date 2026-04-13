using Assets.Audio;
using Assets.Effects.StatusEffects;
using Assets.ElementalSystem;
using Assets.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public interface IPartyCharacter : IAudioClipSource
{
    IStatusEffectReceiver StatusEffectReceiver { get; }
    CharacterConfigSO Config { get; }
    string Name { get; }

    void Initialize(CharacterConfigSO config);

    bool TryPlayAttackAnimationForElement(Elements cardElement);
}

public class PartyCharacter : MonoBehaviour, IPartyCharacter
{
    [field: SerializeField] public CharacterConfigSO Config { get; private set; }

    [SerializeField] private Image _image;

    public IAudioClipPlayer AudioClipPlayer { get; private set; }
    public IStatusEffectReceiver StatusEffectReceiver { get; private set; }
    public string Name => Config.Name;

    private IPartyCharacterAnimationPlayer _animationPlayer;

    public void Initialize(CharacterConfigSO config)
    {
        Config = config;
        _image.sprite = Config.Sprite;
        _animationPlayer?.SetAnimatorController(Config.AnimatorController);
    }

    public bool TryPlayAttackAnimationForElement(Elements cardElement)
    {
        if (Config == null || Config.Element != cardElement)
        {
            return false;
        }

        return _animationPlayer != null && _animationPlayer.TryPlayAttack();
    }

    private void Awake()
    {
        AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
        StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
        _animationPlayer = GetComponentInChildren<IPartyCharacterAnimationPlayer>();
    }
}
