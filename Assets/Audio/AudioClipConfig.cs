using System;
using UnityEngine;

namespace Assets.Audio
{
    [Serializable]
    public class AudioClipConfig
    {
        public AudioClip AudioClip;
        public float Volume = 0.5f;
        public float Pitch = 1f;
        public bool Loop = true;
    }
}
