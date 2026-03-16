using Assets.CustomTypes;
using Assets.Targeting;
using UnityEngine;

namespace Assets.Cards.Base.Damage
{
    [CreateAssetMenu(fileName = "New Card Damage", menuName = "Scriptable Objects/Cards/Card Damage")]
    public class CardDamageSO : ScriptableObject
    {
        [field: SerializeField] public int DamageValue { get; private set; }
        [field: SerializeField] public int AttackCount { get; private set; }
        [field: SerializeField] public StartPosition StartPosition { get; private set; }
        [field: SerializeField] public TargetingMode TargetMode { get; private set; }
    }
}
