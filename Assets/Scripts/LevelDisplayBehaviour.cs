using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelDisplayBehaviour : MonoBehaviour
{
    [SerializeField] private LevelBehaviour _level;
    [SerializeField] private RoomButtonBehaviour _iconRef;
    [SerializeField] private EventSystem _eventSystem;
    private RoomButtonBehaviour[,] _roomButtons;
    private RoomButtonBehaviour _selectedButton;
    private UnityAction<bool> _toggleButtons;
    private bool _focusActive;
    private List<RoomButtonBehaviour> _selectedButtonNeighbors;

    // Start is called before the first frame update
    void Start()
    {
        _roomButtons = new RoomButtonBehaviour[_level.Width, _level.Height];
        DisplayLevelLayout();
    }

    private List<RoomButtonBehaviour> GetButtonNeighbors(int x, int y)
    {
        List<RoomButtonBehaviour> buttons = new List<RoomButtonBehaviour>();

        if (x > 0 && _roomButtons[x - 1,y].enabled)
            buttons.Add(_roomButtons[x - 1, y]);

        if (x < _level.Width - 1 && _roomButtons[x + 1, y].enabled)
            buttons.Add(_roomButtons[x + 1, y]);

        if (y > 0 && _roomButtons[x, y - 1].enabled)
            buttons.Add(_roomButtons[x, y - 1]);

        if (y < _level.Height - 1 && _roomButtons[x, y + 1].enabled)
            buttons.Add(_roomButtons[x, y + 1]);

        return buttons;
    }

    public void ToggleInteractable(int x, int y, bool isInteractable)
    {
        if (_roomButtons[x, y] == _selectedButton)
            return;

        _roomButtons[x,y].interactable = isInteractable;
    }

    public void UpdateSelection(int x, int y)
    {
        if (!_level.PlayerPath.Contains(_level.RoomGraph.GetNode(x, y)))
        {
            _roomButtons[x, y].OnDeselect(null);
            return;
        }

        _focusActive = true;    
        _selectedButton = _roomButtons[x, y];

        _toggleButtons?.Invoke(false);

        _selectedButtonNeighbors = GetButtonNeighbors(x, y);

        foreach (RoomButtonBehaviour button in _selectedButtonNeighbors)
        {
            if (!button.AddedToPath && button.enabled)
                button.interactable = true;
            else if (button.AddedToPath)
                continue;

            button.OnPointerEnter(null);
        }
    }

    public void AddNodeToPath(int x, int y)
    {
        if (!_focusActive)
            return;

        _focusActive = false;
        _toggleButtons?.Invoke(true);

        if (_level.AddNodeToPlayerPath(x, y))
            _roomButtons[x, y].OnAddedToPath?.Invoke();

        foreach (RoomButtonBehaviour button in _selectedButtonNeighbors)
        {
            if (!button.AddedToPath)
                button.OnPointerExit(null);
        }
    }

    void DisplayLevelLayout()
    {
        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                _roomButtons[x, y] = Instantiate(_iconRef, gameObject.transform);
                _roomButtons[x, y].name = "(" + x + "," + y + ")";

                int posX = x;
                int posY = y;

                _toggleButtons += isInteractable => ToggleInteractable(posX, posY, isInteractable);
                _roomButtons[x, y].onClick.AddListener(() => UpdateSelection(posX, posY));
                _roomButtons[x, y].OnButtonSelect += () => AddNodeToPath(posX, posY);
                AssignColor(x, y);
            }
        }

        _eventSystem.firstSelectedGameObject = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y].gameObject;
    }

    private void AssignColor(int x, int y)
    {
        if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "Start")
        {
            _roomButtons[x, y].image.color = Color.green;
            _level.AddNodeToPlayerPath(x, y);
        }
        else if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "End")
            _roomButtons[x, y].image.color = Color.red;
        else if (_level.RoomGraph.GetNode(x, y).Data.inkColor == "Black")
        {
            _roomButtons[x, y].image.color = Color.black;
            _roomButtons[x, y].enabled = false;
        }
        else
            _roomButtons[x, y].image.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
