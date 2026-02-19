using Assets.Audio;
using Assets.Enemies.Base;
using Assets.Scripts.DeathHandlers;
using Assets.Scripts.HealthSystem;
using UnityEngine;

namespace Assets.Scripts.Enemies
{
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyDeathHandler : DeathHandlerBase
    {
        private IEnemyBase _enemy;

        protected override IHealth Health => _enemy.Health;
        protected override IAudioClipPlayer AudioClipPlayer => _enemy.AudioClipPlayer;

        private void Awake()
        {
            _enemy = GetComponent<IEnemyBase>();
        }

        protected override void HandleDeath()
        {
            _enemy.Destroy();
        }
    }
}
