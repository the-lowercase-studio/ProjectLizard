using Assets.Audio;
using Assets.Effects.StatusEffects;
using Assets.Interfaces;
using Assets.Interfaces.Combat;
using Assets.Scripts.HealthSystem;
using Assets.Targeting;
using Assets.VFX;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour, IHealthy, ITarget, IDamageable
{
    [field: SerializeField] public GameObject Visual { get; private set; }
    [field: SerializeField] public EnemyConfigSO Config { get; private set; }
    [SerializeField] private VFXPlayer _damageVfxPlayer;
    private Image _enemyImage;

    public event EventHandler OnCanBeDestroyed;

    public IHealth Health { get; private set; }
    public IAudioClipPlayer AudioClipPlayer { get; private set; }

    public IDamageable Damageable => this;

    public IStatusEffectReceiver StatusEffectReceiver { get; private set; }

    private INeedToCompleteBeforeDisable _enemyDeathSequence;
    public string Name => Config.name;

    protected virtual void Awake()
    {
        Health = GetComponent<IHealth>();
        AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
        StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
        _enemyDeathSequence = GetComponent<INeedToCompleteBeforeDisable>();
        _enemyImage = Visual.GetComponent<Image>();
    }

    protected virtual void OnEnable()
    {
        _enemyDeathSequence.OnCompleted += EnemyDeathSequence_OnCompleted;

        _enemyImage.sprite = Config.Sprite;

        Health.Initialize(Config.MaxHealth);
    }

    protected virtual void OnDisable()
    {
        _enemyDeathSequence.OnCompleted -= EnemyDeathSequence_OnCompleted;
    }

    public virtual void TakeFullHpDamage()
    {
        Health.DecreaseHealth(Health.MaxHealth);
    }

    public virtual void TakeDamage(int damage)
    {
        Health.DecreaseHealth(damage);

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
