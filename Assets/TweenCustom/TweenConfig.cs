using Assets.Cards.Constants;
using System;

namespace Assets.TweenCustom
{
    public readonly struct TweenConfig
    {
        public readonly float TweenDuration;
        public readonly Action Callback;

        public TweenConfig(float tweenDuration = MovementConstants.Tween.DEFAULT_CARD_MOVEMENT_DURATION, Action callback = null)
        {
            TweenDuration = tweenDuration;
            Callback = callback;
        }
    }
}
