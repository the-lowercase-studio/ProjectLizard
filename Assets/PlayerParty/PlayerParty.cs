using Assets.Audio;
using Assets.Constants;
using Assets.DamageNumbers;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using Assets.Interfaces.Combat;
using Assets.Scripts.HealthSystem;
using Assets.ShieldSystem;
using Assets.Targeting;
using Assets.Turns;
using Assets.UI;
using Assets.VFX;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IPlayerParty : ITarget, IDamageable, IShielded, IAudioClipSource
{
    event EventHandler OnPartyDestroyed;

    List<PartyCharacter> GetAllCharacters();

    void AddCharacter(CharacterConfigSO characterConfig);
}

public sealed class PlayerParty : MonoBehaviour, IPlayerParty
{
    public IHealth Health { get; private set; }
    public IShieldReceiver ShieldReceiver { get; private set; }
    public IAudioClipPlayer AudioClipPlayer { get; private set; }
    public IDamageable Damageable => this;
    public IStatusEffectReceiver StatusEffectReceiver { get; private set; }
    public string Name => "Player Party";

    public event EventHandler OnPartyDestroyed;

    [Inject] private ITurnManager _turnManager;
    [Inject] private IDamageNumbers2DSpawner _damageNumbersSpawner;

    [Header("Character Setup")]
    [SerializeField] private CharacterConfigSO _mainCharacterConfig;
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private Transform _partyContainer;

    [Header("Visual Positioning")]
    [SerializeField] private float _circleRadius = 150f;
    [SerializeField] private Vector2 _centerPosition = Vector2.zero;

    [SerializeField] private VFXPlayer _damageVfxPlayer;

    private INeedToCompleteBeforeDisable _partyDeathSequence;
    private List<PartyCharacter> _partyCharacters = new();

    private void Awake()
    {
        Health = GetComponent<IHealth>();
        AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
        StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
        ShieldReceiver = GetComponent<IShieldReceiver>();
        _partyDeathSequence = GetComponent<INeedToCompleteBeforeDisable>();
    }

    private void OnEnable()
    {
        _partyDeathSequence.OnCompleted += PartyDeathSequence_OnCompleted;
        _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
        _turnManager.OnPlayerTurnEnd += TurnManager_OnPlayerTurnEnd;

        SpawnMainCharacter();
    }

    private void OnDisable()
    {
        _partyDeathSequence.OnCompleted -= PartyDeathSequence_OnCompleted;

        _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
        _turnManager.OnPlayerTurnEnd -= TurnManager_OnPlayerTurnEnd;
    }

    private void SpawnMainCharacter()
    {
        PartyCharacter mainCharacter = SpawnCharacter(_mainCharacterConfig);
        _partyCharacters.Add(mainCharacter);

        Health.Initialize(_mainCharacterConfig.HealthContribution);
        RepositionAllCharacters();
    }

    public void AddCharacter(CharacterConfigSO characterConfig)
    {
        PartyCharacter newCharacter = SpawnCharacter(characterConfig);
        _partyCharacters.Add(newCharacter);

        int oldMaxHealth = Health.MaxHealth;
        int currentHealth = Health.CurrentHealth;
        int additionalHealth = characterConfig.HealthContribution;
        int newMaxHealth = oldMaxHealth + additionalHealth;

        Health.Initialize(newMaxHealth);

        AddImageFromCharacterToHitMaterialFlasher(newCharacter);

        int healthToRestore = currentHealth + additionalHealth;
        if (healthToRestore < newMaxHealth)
        {
            int healthToDecrease = newMaxHealth - healthToRestore;
            Health.DecreaseHealth(healthToDecrease);
        }

        RepositionAllCharacters();
    }

    private PartyCharacter SpawnCharacter(CharacterConfigSO config)
    {
        GameObject characterObj = Instantiate(_characterPrefab, _partyContainer);
        PartyCharacter character = characterObj.GetComponent<PartyCharacter>();
        character.Initialize(config);

        return character;
    }

    private void AddImageFromCharacterToHitMaterialFlasher(PartyCharacter newCharacter)
    {
        if (newCharacter.TryGetComponent(out IHitMaterialFlasher hitMaterialFlasher))
        {
            Image image = newCharacter.GetComponentInChildren<Image>();

            if (image != null)
            {
                hitMaterialFlasher.AddImageTarget(image);
            }
        }
    }

    private void RepositionAllCharacters()
    {
        int characterCount = _partyCharacters.Count;

        for (int i = 0; i < characterCount; i++)
        {
            Vector2 position = CalculateCharacterPosition(i, characterCount);
            RectTransform rectTransform = _partyCharacters[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
        }
    }

    private Vector2 CalculateCharacterPosition(int index, int totalCount)
    {
        if (totalCount == 1)
        {
            return _centerPosition;
        }

        float angleStep = 360f / totalCount;
        float angle = angleStep * index;
        float angleInRadians = angle * Mathf.Deg2Rad;

        float x = _centerPosition.x + _circleRadius * Mathf.Cos(angleInRadians);
        float y = _centerPosition.y + _circleRadius * Mathf.Sin(angleInRadians);

        return new Vector2(x, y);
    }

    private void PartyDeathSequence_OnCompleted(object sender, EventArgs e)
    {
        OnPartyDestroyed?.Invoke(this, EventArgs.Empty);
    }

    private void TurnManager_OnPlayerTurnStart(object sender, EventArgs e)
    {
        if (Health.IsAlive())
        {
            OnPlayerTurnStarted();
            ShieldReceiver?.ClearShield();
        }
    }

    private void TurnManager_OnPlayerTurnEnd(object sender, EventArgs e)
    {
        if (Health.IsAlive())
        {
            OnPlayerTurnEnded();
        }
    }

    private void OnPlayerTurnStarted()
    {
    }

    private void OnPlayerTurnEnded()
    {
    }

    public void TakeFullHpDamage()
    {
        Health.DecreaseHealth(Health.MaxHealth);
    }

    public void TakeDamage(int damage)
    {
        int remainingDamage = damage;
        int shieldDamage = 0;

        if (ShieldReceiver != null && ShieldReceiver.HasShield())
        {
            remainingDamage = ShieldReceiver.ReduceShield(damage);
            shieldDamage = Mathf.Max(0, damage - remainingDamage);
        }

        bool shouldSplitPopups = shieldDamage > 0 && remainingDamage > 0;
        float shieldPopupAngle = UnityEngine.Random.Range(
            DamageNumberConstants.Movement.SPLIT_POPUP_LEFT_MIN_ANGLE,
            DamageNumberConstants.Movement.SPLIT_POPUP_LEFT_MAX_ANGLE);
        float healthPopupAngle = UnityEngine.Random.Range(
            DamageNumberConstants.Movement.SPLIT_POPUP_RIGHT_MIN_ANGLE,
            DamageNumberConstants.Movement.SPLIT_POPUP_RIGHT_MAX_ANGLE);

        if (shieldDamage > 0)
        {
            Transform popupTarget = _partyContainer != null ? _partyContainer : transform;
            _damageNumbersSpawner?.SpawnAtTarget(
                popupTarget,
                new DamageNumbers2DSpawnerConfig(
                    shieldDamage,
                    DamageNumberSpawnPattern.UpperHalf,
                    DamageNumberType.Shield,
                    shouldSplitPopups ? shieldPopupAngle : null));
        }

        if (remainingDamage > 0)
        {
            Health.DecreaseHealth(remainingDamage);

            Transform popupTarget = _partyContainer != null ? _partyContainer : transform;
            _damageNumbersSpawner?.SpawnAtTarget(
                popupTarget,
                new DamageNumbers2DSpawnerConfig(
                    remainingDamage,
                    DamageNumberSpawnPattern.UpperHalf,
                    DamageNumberType.Health,
                    shouldSplitPopups ? healthPopupAngle : null));
        }

        if (Health.IsAlive())
        {
            _damageVfxPlayer.Play(new VFXPlayConfig());
        }
    }

    public List<PartyCharacter> GetAllCharacters()
    {
        return _partyCharacters;
    }
}
