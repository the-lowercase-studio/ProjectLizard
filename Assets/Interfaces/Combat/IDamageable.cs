namespace Assets.Interfaces.Combat
{
    public interface IDamageable
    {
        public void TakeDamage(int damage);

        public void TakeFullHpDamage();
    }
}
