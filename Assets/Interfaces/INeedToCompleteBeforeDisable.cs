using System;

namespace Assets.Interfaces
{
    public interface INeedToCompleteBeforeDisable
    {
        public event EventHandler OnCompleted;
    }
}
