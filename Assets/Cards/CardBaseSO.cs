using Assets.ElementalSystem;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace Assets.Cards
{
    [CreateAssetMenu(fileName = "CardBaseSO", menuName = "Scriptable Objects/CardBaseSO")]
    public class CardBaseSO : ScriptableObject
    {
        [field: SerializeField][MaxLength(18)] public string Title { get; private set; }
        [field: SerializeField][MaxLength(76)][TextArea(1, 2)] public string Description { get; private set; }
        [field: SerializeField] public Elements Element { get; private set; }
        [field: SerializeField][Range(0, 9)] public byte StartCost { get; private set; }
        [field: SerializeField] public Sprite CardFront { get; private set; }
    }
}
