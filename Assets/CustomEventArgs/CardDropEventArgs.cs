using Assets.Cards.Base;

namespace Assets.CustomEventArgs
{
    public class CardDropEventArgs
    {
        public Card DroppedCard { get; private set; }

        public CardDropEventArgs(Card droppedCard)
        {
            DroppedCard = droppedCard;
        }
    }
}