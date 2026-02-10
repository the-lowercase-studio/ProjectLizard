namespace Assets.Cards.Constants
{
    public static class CardConstants
    {
        public static class Hand
        {
            public const int START_CARDS_NUMBER = 5;
            public const int MAX_CARDS_NUMBER = 9;

            public static class Placement
            {
                public const int Y_OFFSET = 64;
            }
        }

        public static class Movement
        {
            public const float HOVERED_CARD_Y_OFFSET = 40f;
        }

        public static class Scaling
        {
            public const float DEFAULT_SCALING_FACTOR = 1.14f;
            public const float SCALING_DURATION = 0.3f;
        }
    }
}
