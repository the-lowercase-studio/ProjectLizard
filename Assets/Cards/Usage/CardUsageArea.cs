using Assets.Cards.Base;
using Assets.CustomEventArgs;
using Assets.Extensions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUsageArea : MonoBehaviour, Assets.Interfaces.Interactions.IDropHandler
{
    public static CardUsageArea Instance { get; private set; }

    private EventTrigger _eventTrigger;

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

        _eventTrigger = GetComponent<EventTrigger>();
    }

    private void OnEnable()
    {
        OnDrop += CardUsageArea_OnDrop;

        _eventTrigger.triggers.Clear();
        _eventTrigger.triggers.AddEventHandlerInvocation(OnDrop, EventTriggerType.Drop, this);
    }

    private void OnDisable()
    {
        OnDrop -= CardUsageArea_OnDrop;
    }

    private void CardUsageArea_OnDrop(object sender, PointerEventData e)
    {
        GameObject draggable = e.pointerDrag;

        if (draggable == null)
        {
            return;
        }

        Debug.Log("OnDrop event detected in CardUsageArea. Object: " + draggable.name);

        if (draggable.TryGetComponent(out Card card))
        {
            OnCardDrop?.Invoke(this, new CardDropEventArgs(card));
            Debug.Log($"Card '{card.Config.Title}' dropped in CardUsageArea.");
        }
    }
}