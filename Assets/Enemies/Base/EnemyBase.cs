using Assets.Audio;
using Assets.Constants;
using Assets.DamageNumbers;
using Assets.Effects.StatusEffects;
using Assets.Enemies.Intentions;
using Assets.Enemies.UI;
using Assets.Interfaces;
using Assets.Interfaces.Combat;
using Assets.Scripts.HealthSystem;
using Assets.ShieldSystem;
using Assets.Targeting;
using Assets.Turns;
using Assets.VFX;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Enemies.Base
{
    public interface IEnemyBase : ITarget, IDamageable, IShielded
    {
        IAudioClipPlayer AudioClipPlayer { get; }
        EnemyConfigSO Config { get; }
        GameObject Visual { get; }

        event EventHandler OnCanBeDestroyed;

        void Destroy();
    }

    public class EnemyBase : MonoBehaviour, IEnemyBase, IParalyzable
    {
        [field: SerializeField] public GameObject Visual { get; private set; }
        [field: SerializeField] public EnemyConfigSO Config { get; private set; }

        public IHealth Health { get; private set; }
        public IShieldReceiver ShieldReceiver { get; private set; }
        public IAudioClipPlayer AudioClipPlayer { get; private set; }
        public IDamageable Damageable => this;
        public IStatusEffectReceiver StatusEffectReceiver { get; private set; }
        public string Name => Config.name;

        public event EventHandler OnCanBeDestroyed;

        [Inject] private ITurnManager _turnManager;
        [Inject] private IPlayerParty _playerParty;
        [Inject] private IDamageNumbers2DSpawner _damageNumbersSpawner;

        [SerializeField] private VFXPlayer _damageVfxPlayer;
        [SerializeField] private IntentionIndicator _intentionIndicator;

        private Image _enemyImage;
        private INeedToCompleteBeforeDisable _enemyDeathSequence;
        private IEnemyAnimationPlayer _enemyIntentAnimationPlayer;
        private IntentionSelector _intentionSelector;
        private IntentionConfig _currentIntention;
        private bool _isParalysed;

        protected virtual void Awake()
        {
            Health = GetComponent<IHealth>();
            AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
            StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
            ShieldReceiver = GetComponent<IShieldReceiver>();
            _enemyDeathSequence = GetComponent<INeedToCompleteBeforeDisable>();
            _enemyIntentAnimationPlayer = GetComponentInChildren<IEnemyAnimationPlayer>();
            _enemyImage = Visual.transform.GetChild(0).GetComponent<Image>();
            _intentionSelector = new IntentionSelector();
        }

        protected virtual void OnEnable()
        {
            _enemyDeathSequence.OnCompleted += EnemyDeathSequence_OnCompleted;
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
            _turnManager.OnEnemyTurnStart += TurnManager_OnEnemyTurnStart;
            _turnManager.OnEnemyTurnEnd += TurnManager_OnEnemyTurnEnd;

            _enemyImage.sprite = Config.Sprite;

            Health.Initialize(Config.MaxHealth);
        }

        protected virtual void OnDisable()
        {
            _enemyDeathSequence.OnCompleted -= EnemyDeathSequence_OnCompleted;

            _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
            _turnManager.OnEnemyTurnStart -= TurnManager_OnEnemyTurnStart;
            _turnManager.OnEnemyTurnEnd -= TurnManager_OnEnemyTurnEnd;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void TurnManager_OnPlayerTurnStart(object sender, EventArgs e)
        {
            if (!Health.IsAlive())
            {
                return;
            }

            if (_isParalysed)
            {
                _intentionIndicator?.ShowActionIntention(new IntentionConfig(IntentionType.SelfParalysis, 0, null));
                return;
            }

            SelectIntention();
        }

        private void TurnManager_OnEnemyTurnEnd(object sender, EventArgs e)
        {
            if (Health.IsAlive())
            {
                ExecuteIntention();
            }
        }

        private void TurnManager_OnEnemyTurnStart(object sender, EventArgs e)
        {
            if (!Health.IsAlive())
            {
                return;
            }

            ShieldReceiver?.ClearShield();
        }

        private void SelectIntention()
        {
            if (Config.Intentions != null && Config.Intentions.Count > 0)
            {
                _currentIntention = _intentionSelector.SelectIntention(Config.Intentions);
                _currentIntention.Action.RefreshValue();

                if (_currentIntention != null)
                {
                    Debug.Log($"{Name} selected intention: {_currentIntention.IntentionType}");

                    if (_intentionIndicator != null)
                    {
                        _intentionIndicator.ShowActionIntention(_currentIntention);
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
            if (_isParalysed)
            {
                Debug.Log($"{Name} is stunned and skips the turn.");
                _currentIntention = null;
                return;
            }

            if (_currentIntention?.Action != null)
            {
                _enemyIntentAnimationPlayer?.TryPlayForIntention(_currentIntention.IntentionType);
                _currentIntention.Action.Execute(this, _playerParty);
                _currentIntention = null;
            }
        }

        public void ApplyParalysis()
        {
            _isParalysed = true;
            _currentIntention = null;
            _intentionIndicator?.ShowActionIntention(new IntentionConfig(IntentionType.SelfParalysis, 0, null));
        }

        public void RemoveParalysis()
        {
            _isParalysed = false;
        }

        public virtual void TakeFullHpDamage()
        {
            Health.DecreaseHealth(Health.MaxHealth);
        }

        public virtual void TakeDamage(int damage)
        {
            Debug.Log($"{this.gameObject.name} taken damage for {damage}");

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
                Transform popupTarget = Visual != null ? Visual.transform : transform;
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

                Transform popupTarget = Visual != null ? Visual.transform : transform;
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

        private void EnemyDeathSequence_OnCompleted(object sender, EventArgs e)
        {
            OnCanBeDestroyed?.Invoke(this, EventArgs.Empty);
        }
    }
}
