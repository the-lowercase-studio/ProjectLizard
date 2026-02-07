using Assets.Audio;

namespace Assets.Interfaces
{
    public interface IAudioClipSource
    {
        IAudioClipPlayer AudioClipPlayer { get; }
    }
}
