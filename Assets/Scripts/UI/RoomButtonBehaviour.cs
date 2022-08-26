using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class RoomButtonBehaviour : Button, ISelectHandler, IPointerDownHandler
{
    [SerializeField] private UnityAction _onSelect;
    [SerializeField] private UnityAction _onPointerDown;

    public UnityAction OnButtonSelect { get => _onSelect; set => _onSelect = value; }
    public UnityAction OnMousePointerDown { get => _onPointerDown; set => _onPointerDown = value; }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        _onSelect?.Invoke();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _onPointerDown?.Invoke();
    }
}
