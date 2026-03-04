using Assets.Audio;
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

    public class EnemyBase : MonoBehaviour, IEnemyBase
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

        [SerializeField] private VFXPlayer _damageVfxPlayer;
        [SerializeField] private IntentionIndicator _intentionIndicator;

        private Image _enemyImage;
        private INeedToCompleteBeforeDisable _enemyDeathSequence;
        private IntentionSelector _intentionSelector;
        private IntentionConfig _currentIntention;

        protected virtual void Awake()
        {
            Health = GetComponent<IHealth>();
            AudioClipPlayer = GetComponentInChildren<IAudioClipPlayer>();
            StatusEffectReceiver = GetComponent<IStatusEffectReceiver>();
            ShieldReceiver = GetComponent<IShieldReceiver>();
            _enemyDeathSequence = GetComponent<INeedToCompleteBeforeDisable>();
            _enemyImage = Visual.transform.GetChild(0).GetComponent<Image>();
            _intentionSelector = new IntentionSelector();
        }

        protected virtual void OnEnable()
        {
            _enemyDeathSequence.OnCompleted += EnemyDeathSequence_OnCompleted;
            _turnManager.OnPlayerTurnStart += TurnManager_OnPlayerTurnStart;
            _turnManager.OnEnemyTurnEnd += TurnManager_OnEnemyTurnEnd;

            _enemyImage.sprite = Config.Sprite;

            Health.Initialize(Config.MaxHealth);
        }

        protected virtual void OnDisable()
        {
            _enemyDeathSequence.OnCompleted -= EnemyDeathSequence_OnCompleted;

            _turnManager.OnPlayerTurnStart -= TurnManager_OnPlayerTurnStart;
            _turnManager.OnEnemyTurnEnd -= TurnManager_OnEnemyTurnEnd;
        }

        public void Destroy()
        {
            Destroy(gameObject);
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
                _currentIntention.Action.RefreshValue();

                if (_currentIntention != null)
                {
                    Debug.Log($"{Name} selected intention: {_currentIntention.IntentionType}");

                    if (_intentionIndicator != null)
                    {
                        _intentionIndicator.ShowIntention(_currentIntention);
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
            if (_currentIntention?.Action != null)
            {
                _currentIntention.Action.Execute(this, _playerParty);
                _currentIntention = null;
            }
        }

        public virtual void TakeFullHpDamage()
        {
            Health.DecreaseHealth(Health.MaxHealth);
        }

        public virtual void TakeDamage(int damage)
        {
            Debug.Log($"{this.gameObject.name} taken damage for {damage}");

            int remainingDamage = damage;

            if (ShieldReceiver != null && ShieldReceiver.HasShield())
            {
                remainingDamage = ShieldReceiver.ReduceShield(damage);
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
}
