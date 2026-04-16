namespace Assets.Cards.Base.Interaction
{
    public interface ICardDragLock
    {
        bool IsAnyCardBeingDragged { get; }

        bool TryAcquire();

        void Release();
    }

    public class CardDragLock : ICardDragLock
    {
        public bool IsAnyCardBeingDragged { get; private set; }

        public bool TryAcquire()
        {
            if (IsAnyCardBeingDragged)
            {
                return false;
            }

            IsAnyCardBeingDragged = true;
            return true;
        }

        public void Release()
        {
            IsAnyCardBeingDragged = false;
        }
    }
}
