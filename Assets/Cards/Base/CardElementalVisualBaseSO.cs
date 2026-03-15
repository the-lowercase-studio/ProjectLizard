using UnityEngine;

namespace Assets.Cards.Base
{
    [CreateAssetMenu(fileName = "CardElementalVisualBaseSO", menuName = "Cards/Scriptable Objects/CardElementalVisualBaseSO")]
    public class CardElementalVisualBaseSO : ScriptableObject
    {
        [field: SerializeField] public Sprite TitleBackground { get; private set; }
        [field: SerializeField] public Sprite DescriptionBackground { get; private set; }
        [field: SerializeField] public Sprite Background { get; private set; }
    }
}