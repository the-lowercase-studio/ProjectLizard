using Assets.Cards.Base.Damage;
using Assets.Effects.Base;
using Assets.ElementalSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Cards.Base
{
    [CreateAssetMenu(fileName = "CardConfigBaseSO", menuName = "Cards/Scriptable Objects/CardConfigBaseSO")]
    public class CardConfigBaseSO : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField][TextArea(1, 2)] public string Description { get; private set; }
        [field: SerializeField][Range(0, 9)] public byte StartEnergyCost { get; private set; }
        [field: SerializeField] public Elements Element { get; private set; }
        [field: SerializeField] public CardElementalVisualBaseSO ElementalVisualBase { get; private set; }
        [field: SerializeField] public Sprite FrontGraphic { get; private set; }
        [field: SerializeField] public CardDamageSO Damage { get; private set; }
        [field: SerializeField] public List<EffectSO> Effects { get; private set; } = new();
    }
}
