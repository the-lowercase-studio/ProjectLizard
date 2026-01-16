using System;

namespace Assets.Combat.Shield
{
    public interface IShielded
    {
        public IShield Shield { get; }
    }

    public interface IShield
    {
        int CurrentShield { get; }

        event EventHandler OnShieldChanged;

        event EventHandler OnShieldDepleted;

        event EventHandler OnShieldGained;

        void AddShield(int amount);

        int ReduceShield(int amount);

        void ClearShield();

        bool HasShield();
    }
}
