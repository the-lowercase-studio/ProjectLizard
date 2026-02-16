using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Effects.UI
{
    [CreateAssetMenu(fileName = "EffectTypeMapping", menuName = "Effects/Effect Type Sprite Mapping")]
    public class EffectTypeSpriteMappingSO : ScriptableObject
    {
        [SerializeField] private List<EffectTypeMapping> _mappings = new List<EffectTypeMapping>();

        public Sprite GetSpriteForEffectType(EffectType effectType)
        {
            EffectTypeMapping mapping = _mappings.Find(m => m.effectType == effectType);
            return mapping?.sprite;
        }
    }

    [Serializable]
    public class EffectTypeMapping
    {
        public EffectType effectType;
        public Sprite sprite;
    }
}
