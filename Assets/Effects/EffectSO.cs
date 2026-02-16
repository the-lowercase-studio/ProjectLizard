using Assets.Targeting;
using UnityEngine;

namespace Assets.Effects
{
    public abstract class EffectSO : ScriptableObject
    {
        [field: SerializeField] public string EffectName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public EffectType EffectType { get; private set; }

        public abstract void Execute(CardEffectContext context);
    }

    public class CardEffectContext
    {
        public ITarget Target { get; set; }
        public GameObject Source { get; set; }
        public Vector3 Position { get; set; }
    }
}
