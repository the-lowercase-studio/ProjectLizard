using Assets.Cards;
using Assets.CustomEventArgs;
using UnityEngine;

public class CardsPlacement : MonoBehaviour
{
    private CardsHandManager _handManager;

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
        Vector3 cardPos = card.Movement.GetPosition();
        SetCardYOffset(card, cardPos);
        SetCardZRotation(card, cardPos);
    }

    private void SetCardYOffset(ICard card, Vector3 cardPos)
    {
        float yOffset = CalculateYOffset(cardPos.x);
        card.Movement.SetPosition(new Vector3(cardPos.x, yOffset));
    }

    private float CalculateYOffset(float xPos)
    {
        //y = -1/8x^2 + 10
        return (-0.125f * xPos * xPos) + 10f;
    }

    private void SetCardZRotation(ICard card, Vector3 cardPos)
    {
        Vector3 rotation = card.Rotation.GetVisualEulerRotation();
        float zRotation = CalculateZRotation(cardPos.x);
        card.Rotation.SetZVisualRotation(zRotation);
    }

    private float CalculateZRotation(float xPos)
    {
        //y = -1/24x^2 + 10
        return (-0.0416f * xPos * xPos) + 10f;
    }
}
