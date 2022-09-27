using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class RoomButtonBehaviour : Button, ISelectHandler, IPointerDownHandler
{
    [SerializeField] private UnityAction _onSelect;
    [SerializeField] private UnityAction _onDeselect;
    [SerializeField] private UnityAction _onPointerDown;
    [SerializeField] private UnityAction _onAddedToPath;
    [SerializeField] private UnityAction _onRemovedFromPath;
    [SerializeField] private bool _addedToPath;
    [SerializeField] private bool _markedForRemoval;
    [SerializeField] private Color _pathColor;
    private Vector2 _position;

    public UnityAction OnButtonSelect { get => _onSelect; set => _onSelect = value; }
    public UnityAction OnButtonDeselect { get => _onDeselect; set => _onDeselect = value; }
    public UnityAction OnMousePointerDown { get => _onPointerDown; set => _onPointerDown = value; }
    public UnityAction OnAddedToPath { get => _onAddedToPath; set => _onAddedToPath = value; }
    public bool AddedToPath { get => _addedToPath; set => _addedToPath = value; }
    public Vector2 Position { get => _position; set => _position = value; }
    public UnityAction OnRemovedFromPath { get => _onRemovedFromPath; set => _onRemovedFromPath = value; }
    public bool MarkedForRemoval { get => _markedForRemoval; set => _markedForRemoval = value; }

    protected override void Awake()
    {
        _pathColor = Color.cyan;
        OnAddedToPath += () => image.color = _pathColor;
        OnRemovedFromPath += () => image.color = Color.white;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        _onSelect?.Invoke();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        _onDeselect?.Invoke();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _onPointerDown?.Invoke();
    }
}
