using DG.Tweening;
using UnityEngine;

namespace Assets.TweenCustom
{
    public static class UIShakeEffects
    {
        public static Tween WeakShake(Transform transform)
        {
            const float duration = 0.1f, strength = 0.10f, randomness = 90f;
            const int vibratio = 3;
            const bool snapping = true, fadeOut = true;

            return transform.DOShakePosition(duration,
                                      strength,
                                      vibratio,
                                      randomness,
                                      snapping,
                                      fadeOut,
                                      ShakeRandomnessMode.Harmonic);
        }
    }
}
