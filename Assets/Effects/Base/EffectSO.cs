using Assets.Targeting;
using Assets.Turns;
using UnityEngine;

namespace Assets.Effects.Base
{
    public abstract class EffectSO : ScriptableObject
    {
        [field: SerializeField] public string EffectName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public int TurnDuration { get; private set; }
        [field: SerializeField] public bool CanStackValue { get; private set; }
        [field: SerializeField] public EffectType EffectType { get; private set; }
        [field: SerializeField] public TurnExecutionState ExecutionState { get; private set; }

        public abstract void Execute(CardEffectContext context);
    }

    public class CardEffectContext
    {
        public ITarget Target { get; set; }
        public GameObject Source { get; set; }
        public Vector3 Position { get; set; }
        public ITargetsProvider TargetsProvider { get; set; }
    }
}
