using Assets.Cards;
using Assets.CustomEventArgs;
using UnityEngine;
using UnityEngine.UI;

public class CardsInHandPositioner : MonoBehaviour
{
    public static CardsInHandPositioner Instance { get; private set; }

    [Header("Cards defaults")]
    [SerializeField] private byte _startCardsCount = 5;

    [Header("Curves")]
    [SerializeField] private AnimationCurve _positionCurve;
    [SerializeField] private AnimationCurve _rotationCurve;

    [Header("Cards Spacing")]
    [SerializeField][Range(-180, 0)] private float _minCardsSpacing;
    [SerializeField][Range(-180, 0)] private float _maxCardsSpacing;
    [SerializeField][Range(-36, 0)] private float _cardsSpacingDecreaser = -16f;

    private CardsHandManager _handManager;
    private HorizontalLayoutGroup _cardsGroup;

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

        _cardsGroup = GetComponent<HorizontalLayoutGroup>();
    }

    private void Start()
    {
        _handManager = CardsHandManager.Instance;

        UpdateCardsOverlapping();

        _handManager.OnHandChange += HandManager_OnHandChange;
    }

    private void HandManager_OnHandChange(object sender, EnumerableCollectionChangeEventArgs<ICard> e)
    {
        foreach (ICard card in e.CollectionAfterChange)
        {
            UpdateCardPlacement(card);
        }

        UpdateCardsOverlapping();
    }

    public void UpdateCardPlacement(ICard card)
    {
        Vector3 cardPos = card.Movement.GetRectAnchoredPosition();
        SetCardYOffset(card, cardPos);
        SetCardZRotation(card, cardPos);
    }

    private void UpdateCardsOverlapping()
    {
        int cardDiff = _handManager.CountCards() - _startCardsCount;

        if (cardDiff > 0)
        {
            float spacing = _maxCardsSpacing + _cardsSpacingDecreaser * cardDiff;
            if (spacing >= _minCardsSpacing)
            {
                _cardsGroup.spacing = spacing;
            }
        }
        else
        {
            _cardsGroup.spacing = _maxCardsSpacing;
        }
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
