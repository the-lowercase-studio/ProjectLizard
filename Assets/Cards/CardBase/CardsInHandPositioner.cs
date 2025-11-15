using Assets.Cards;
using Assets.CustomEventArgs;
using UnityEngine;

public class CardsInHandPositioner : MonoBehaviour
{
    public static CardsInHandPositioner Instance { get; private set; }

    [SerializeField] private AnimationCurve _positionCurve;
    [SerializeField] private AnimationCurve _rotationCurve;

    private CardsHandManager _handManager;

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

    private void Start()
    {
        _handManager = CardsHandManager.Instance;

        _handManager.OnHandChange += HandManager_OnHandChange;
    }

    private void HandManager_OnHandChange(object sender, EnumerableCollectionChangeEventArgs<ICard> e)
    {
        foreach (ICard card in e.CollectionAfterChange)
        {
            UpdateCardPlacement(card);
        }
    }

    public void UpdateCardPlacement(ICard card)
    {
        Vector3 cardPos = card.Movement.GetRectAnchoredPosition();
        SetCardYOffset(card, cardPos);
        SetCardZRotation(card, cardPos);
    }

    private void SetCardYOffset(ICard card, Vector3 cardPos)
    {
        float yOffset = _positionCurve.Evaluate(cardPos.x);
        card.Movement.SetVisualRectAnchoredPosition(new Vector3(0, yOffset, 0));
    }

    private void SetCardZRotation(ICard card, Vector3 cardPos)
    {
        float zRotation = _rotationCurve.Evaluate(cardPos.x);
        card.Rotation.SetZVisualRotation(zRotation);
    }
}
