using System;
using UnityEngine;

namespace Assets.DamageNumbers
{
    [Serializable]
    public struct DamageNumberAppearance
    {
        public float FontSize;
        public float GrowFontSizeAnimationScaleMultiplier;
        public Color Color;

        public DamageNumberAppearance(float fontSize, float growFontSizeAnimationScaleMultiplier, Color color)
        {
            FontSize = fontSize;
            GrowFontSizeAnimationScaleMultiplier = growFontSizeAnimationScaleMultiplier;
            Color = color;
        }

        public void Deconstruct(out float fontSize, out float growFontSizeAnimationScaleMultiplier, out Color color)
        {
            fontSize = FontSize;
            growFontSizeAnimationScaleMultiplier = GrowFontSizeAnimationScaleMultiplier;
            color = Color;
        }
    }
}
