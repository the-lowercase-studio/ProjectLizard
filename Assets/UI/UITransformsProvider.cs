using UnityEngine;

namespace Assets.UI
{
    public class UITransformsProvider : MonoBehaviour
    {
        public static UITransformsProvider Instance { get; private set; }
        [field: SerializeField] public RectTransform FrontPanel;
        [field: SerializeField] public RectTransform BackPanel;
        [field: SerializeField] public RectTransform CardsHolder;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
