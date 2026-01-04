using DG.Tweening;

namespace Assets.TweenCustom
{
    public static class TweenExtensions
    {
        public static void KillIfPlaying(this Tween tween)
        {
            if (tween?.IsPlaying() == true)
            {
                tween.Kill();
            }
        }
    }
}