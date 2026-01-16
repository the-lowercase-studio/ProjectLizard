using Assets.Scripts.HealthSystem;

namespace Assets.Interfaces.Combat
{
    public interface IDamageable : IHealthy
    {
        public void TakeDamage(int damage);

        public void TakeFullHpDamage();
    }
}
