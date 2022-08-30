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
    [SerializeField] private UnityAction _onAddedToPath;
    [SerializeField] private bool _addedToPath;
    [SerializeField] private Color _pathColor;

    public UnityAction OnButtonSelect { get => _onSelect; set => _onSelect = value; }
    public UnityAction OnMousePointerDown { get => _onPointerDown; set => _onPointerDown = value; }
    public UnityAction OnAddedToPath { get => _onAddedToPath; set => _onAddedToPath = value; }
    public bool AddedToPath { get => _addedToPath; set => _addedToPath = value; }

    protected override void Awake()
    {
        _pathColor = Color.blue;
        OnAddedToPath += () => image.color = _pathColor;
    }

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
