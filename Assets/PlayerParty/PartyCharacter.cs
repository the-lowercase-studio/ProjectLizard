using Assets.Audio;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using UnityEngine;

public interface IPartyCharacter : IAudioClipSource
{
    IStatusEffectReceiver StatusEffectReceiver { get; }
    CharacterConfigSO Config { get; }
    string Name { get; }

    void Initialize(CharacterConfigSO config);
}

public class PartyCharacter : MonoBehaviour, IPartyCharacter
{
    [field: SerializeField] public CharacterConfigSO Config { get; private set; }
    [SerializeField] private Sprite _characterSprite;

    public IAudioClipPlayer AudioClipPlayer { get; private set; }
    public IStatusEffectReceiver StatusEffectReceiver { get; private set; }

    public string Name => Config.Name;

    public void Initialize(CharacterConfigSO config)
    {
        Config = config;
        _characterSprite = Config.Sprite;
    }

    private void Awake()
    {
        AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
        StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
    }
}
