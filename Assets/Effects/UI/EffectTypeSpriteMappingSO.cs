using Assets.Effects.Base;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Effects.UI
{
    [CreateAssetMenu(fileName = "EffectTypeMapping", menuName = "Effects/Effect Type Sprite Mapping")]
    public class EffectTypeSpriteMappingSO : ScriptableObject
    {
        [SerializeField] private List<EffectTypeMapping> _mappings = new List<EffectTypeMapping>();

        public Sprite GetSpriteForEffectType(EffectType effectType)
        {
            EffectTypeMapping mapping = _mappings.Find(m => m.EffectType == effectType);
            return mapping?.Sprite;
        }

        public AnimatorController GetInitialEffectAnimatorForEffectType(EffectType effectType)
        {
            EffectTypeMapping mapping = _mappings.Find(m => m.EffectType == effectType);
            return mapping?.InitialEffectAnimator;
        }
    }

    [Serializable]
    public class EffectTypeMapping
    {
        public EffectType EffectType;
        public Sprite Sprite;
        public AnimatorController InitialEffectAnimator;
    }
}
