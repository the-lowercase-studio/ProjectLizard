namespace Assets.DamageNumbers.Constants
{
    public static class DamageNumberConstants
    {
        public static class Randomization
        {
            public const int RECENT_MOVEMENT_HISTORY_SIZE = 2;
            public const int ANGLE_SELECTION_ATTEMPTS = 8;
            public const float MIN_ANGLE_SEPARATION_DEGREES = 24f;
        }

        public static class Movement
        {
            public const float FULL_CIRCLE_MIN_ANGLE = 0f;
            public const float FULL_CIRCLE_MAX_ANGLE = 360f;
            public const float UPPER_HALF_MIN_ANGLE = 35f;
            public const float UPPER_HALF_MAX_ANGLE = 145f;
            public const float DEFAULT_FALLBACK_ANGLE = 90f;

            public const float SPLIT_POPUP_LEFT_MIN_ANGLE = 105f;
            public const float SPLIT_POPUP_LEFT_MAX_ANGLE = 145f;
            public const float SPLIT_POPUP_RIGHT_MIN_ANGLE = 35f;
            public const float SPLIT_POPUP_RIGHT_MAX_ANGLE = 75f;
        }
    }
}