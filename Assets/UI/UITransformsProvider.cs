using UnityEngine;

namespace Assets.UI
{
    public interface IUITransformsProvider
    {
        RectTransform FrontPanel { get; }
        RectTransform BackPanel { get; }
        RectTransform CardsHolder { get; }
    }

    public class UITransformsProvider : MonoBehaviour, IUITransformsProvider
    {
        [field: SerializeField] public RectTransform FrontPanel { get; private set; }
        [field: SerializeField] public RectTransform BackPanel { get; private set; }
        [field: SerializeField] public RectTransform CardsHolder { get; private set; }
    }
}
