using Assets.Audio;
using Assets.Combat.Shield;
using Assets.Effects.StatusEffects;
using Assets.Enemies.Base.Intentions;
using Assets.Enemies.UI;
using Assets.Interfaces;
using Assets.Interfaces.Combat;
using Assets.Scripts.HealthSystem;
using Assets.Targeting;
using Assets.Turns;
using Assets.VFX;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour, ITarget, IDamageable, IShielded
{
    [field: SerializeField] public GameObject Visual { get; private set; }
    [field: SerializeField] public EnemyConfigSO Config { get; private set; }
    [SerializeField] private VFXPlayer _damageVfxPlayer;
    [SerializeField] private IntentionIndicator _intentionIndicator;
    private Image _enemyImage;

    public event EventHandler OnCanBeDestroyed;

    public IHealth Health { get; private set; }
    public IShield Shield { get; private set; }
    public IAudioClipPlayer AudioClipPlayer { get; private set; }

    public IDamageable Damageable => this;

    public IStatusEffectReceiver StatusEffectReceiver { get; private set; }

    private INeedToCompleteBeforeDisable _enemyDeathSequence;
    public string Name => Config.name;

    private IntentionSelector _intentionSelector;
    private IntentionConfig _currentIntention;

    public IntentionConfig CurrentIntention => _currentIntention;

    protected virtual void Awake()
    {
        Health = GetComponent<IHealth>();
        AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
        StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
        _enemyDeathSequence = GetComponent<INeedToCompleteBeforeDisable>();
        _enemyImage = Visual.GetComponent<Image>();
        _intentionSelector = new IntentionSelector();

        if (_intentionIndicator == null)
        {
            _intentionIndicator = GetComponentInChildren<IntentionIndicator>();
        }
    }

    protected virtual void OnEnable()
    {
        _enemyDeathSequence.OnCompleted += EnemyDeathSequence_OnCompleted;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
            TurnManager.Instance.OnEnemyTurnEnd += TurnManager_OnEnemyTurnEnd;
        }

        _enemyImage.sprite = Config.Sprite;

        Health.Initialize(Config.MaxHealth);
    }

    protected virtual void OnDisable()
    {
        _enemyDeathSequence.OnCompleted -= EnemyDeathSequence_OnCompleted;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
            TurnManager.Instance.OnEnemyTurnEnd -= TurnManager_OnEnemyTurnEnd;
        }
    }

    private void TurnManager_OnPlayerTurnStart(object sender, EventArgs e)
    {
        if (Health.IsAlive())
        {
            SelectIntention();
        }
    }

    private void TurnManager_OnEnemyTurnEnd(object sender, EventArgs e)
    {
        if (Health.IsAlive())
        {
            ExecuteIntention();
        }
    }

    private void SelectIntention()
    {
        if (Config.Intentions != null && Config.Intentions.Count > 0)
        {
            _currentIntention = _intentionSelector.SelectIntention(Config.Intentions);

            if (_currentIntention != null)
            {
                Debug.Log($"{Name} selected intention: {_currentIntention.IntentionType}");

                if (_intentionIndicator != null)
                {
                    _intentionIndicator.ShowIntention(_currentIntention.IntentionType);
                }
            }
            else
            {
                Debug.LogWarning($"{Name} failed to select an intention!");
            }
        }
        else
        {
            Debug.LogWarning($"{Name} has no intentions configured!");
        }
    }

    private void ExecuteIntention()
    {
        if (_currentIntention != null && _currentIntention.Action != null)
        {
            _currentIntention.Action.Execute(this);
            _currentIntention = null;
        }
    }

    public virtual void TakeFullHpDamage()
    {
        Health.DecreaseHealth(Health.MaxHealth);
    }

    public virtual void TakeDamage(int damage)
    {
        int remainingDamage = damage;

        if (Shield != null && Shield.HasShield())
        {
            remainingDamage = Shield.ReduceShield(damage);
        }

        if (remainingDamage > 0)
        {
            Health.DecreaseHealth(remainingDamage);
        }

        if (Health.IsAlive())
        {
            _damageVfxPlayer.Play(new VFXPlayConfig());
        }
    }

    private void EnemyDeathSequence_OnCompleted(object sender, EventArgs e)
    {
        OnCanBeDestroyed?.Invoke(this, EventArgs.Empty);
    }
}
