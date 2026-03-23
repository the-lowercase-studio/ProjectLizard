namespace Assets.Cards.Base.Damage
{
    public readonly struct CardDamagePreviewInfo
    {
        public int DamageValue { get; }
        public int DamageHitCount { get; }

        public CardDamagePreviewInfo(int damageValue, int damageHitCount)
        {
            DamageValue = damageValue;
            DamageHitCount = damageHitCount;
        }
    }
}