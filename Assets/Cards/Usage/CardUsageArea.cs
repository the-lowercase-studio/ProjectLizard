using Assets.Cards.Base;
using Assets.CustomEventArgs;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUsageArea : MonoBehaviour, IDropHandler
{
    public static CardUsageArea Instance { get; private set; }

    public event EventHandler<CardDropEventArgs> OnCardDrop;

    public event EventHandler<PointerEventData> OnDrop;

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

    private void OnEnable()
    {
        OnDrop += CardUsageArea_OnDrop;
    }

    private void OnDisable()
    {
        OnDrop -= CardUsageArea_OnDrop;
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        CardUsageArea_OnDrop(this, eventData);
    }

    private void CardUsageArea_OnDrop(object sender, PointerEventData e)
    {
        GameObject draggable = e.pointerDrag;

        if (draggable == null)
        {
            return;
        }

        Debug.Log("OnDrop event detected in CardUsageArea. Object: " + e);

        if (draggable.TryGetComponent(out Card card))
        {
            OnCardDrop?.Invoke(this, new CardDropEventArgs(card));
            Debug.Log($"Card '{card.Config.Title}' dropped in CardUsageArea.");
        }
    }
}
