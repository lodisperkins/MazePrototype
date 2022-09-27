using DungeonGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelDisplayBehaviour : MonoBehaviour
{
    [Tooltip("A reference to a game object in the scene that will generate the level.")]
    [SerializeField] private LevelBehaviour _level;
    [Tooltip("A reference to the prefab that will be used to represent a room button.")]
    [SerializeField] private RoomButtonBehaviour _iconRef;
    [Tooltip("Unity's UI event system object.")]
    [SerializeField] private EventSystem _eventSystem;
    [Tooltip("A reference to the button that will be used to activate the dungeon generation.")]
    [SerializeField] private GameObject _loadDungeonButton;
    private RoomButtonBehaviour[,] _roomButtons;
    private static RoomButtonBehaviour _selectedButton;
    private UnityAction<bool> _toggleButtons;
    private static bool _focusActive;
    private static bool _eraseActive;
    private List<RoomButtonBehaviour> _selectedButtonNeighbors;

    public static bool EraseActive 
    { 
        get => _eraseActive;
        set
        {
            _eraseActive = value;

            if (_eraseActive)
                _selectedButton.OnButtonSelect();

        }
    }
    public static bool FocusActive { get => _focusActive; set => _focusActive = value; }

    // Start is called before the first frame update
    void Start()
    {
        _roomButtons = new RoomButtonBehaviour[_level.Width, _level.Height];
        DisplayLevelLayout();
    }

    /// <summary>
    /// Gets all buttons in cardinal directions surrounding the button at the given position.
    /// </summary>
    /// <param name="x">The x position of the button to get neighbors for.</param>
    /// <param name="y">The y position of the button to get neighbors for.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Turns on/off the ability for a button to be selected or pressed.
    /// </summary>
    /// <param name="x">The x position of the node to toggle.</param>
    /// <param name="y">The y position of the node to toggle.</param>
    /// <param name="isInteractable">Whether or not this button should be interactable.</param>
    public void SetIsInteractable(int x, int y, bool isInteractable)
    {
        if (_roomButtons[x, y] == _selectedButton)
            return;

        _roomButtons[x,y].interactable = isInteractable;
    }

    /// <summary>
    /// Updates the currently selected button to be the button at the given position
    /// </summary>
    /// <param name="x">The x position of the node to select.</param>
    /// <param name="y">The y position of the node to select.</param>
    public void UpdateSelection(int x, int y)
    {
        if (!_level.PlayerPath.Contains(_level.RoomGraph.GetNode(x, y)))
        {
            _roomButtons[x, y].OnDeselect(null);
            return;
        }

        FocusActive = true;    
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

    /// <summary>
    /// Trys to add the room at the given position to the players path.
    /// </summary>
    /// <param name="x">The x position of the room to add.</param>
    /// <param name="y">The y position of the room to add.</param>
    public bool AddNodeToPath(int x, int y)
    {
        if (!FocusActive)
            return false;

        bool added = false;

        FocusActive = false;
        _toggleButtons?.Invoke(true);

        if (_level.AddNodeToPlayerPath(_selectedButton.Position, new Vector2(x, y)))
        {
            _roomButtons[x, y].OnAddedToPath?.Invoke();
            _roomButtons[x, y].AddedToPath = true;

            if (_level.ExitPosition == new Vector2(x, y))
                _loadDungeonButton.SetActive(true);

            added = true;
        }

        foreach (RoomButtonBehaviour button in _selectedButtonNeighbors)
        {
            button.OnPointerExit(null);
        }

        return added;
    }

    public void MarkNodesForRemoval()
    {
        EraseActive = true;

        _toggleButtons?.Invoke(false);

        List<Node<RoomDescription>> removableNodes = _level.GetRemovableNodes();

        foreach (Node<RoomDescription> button in removableNodes)
        {
            RoomButtonBehaviour buttonDisplay = _roomButtons[(int)button.Position.x, (int)button.Position.y];

            if (buttonDisplay.enabled)
            {
                buttonDisplay.interactable = true;
                buttonDisplay.MarkedForRemoval = true;
            }

            //buttonDisplay.OnPointerEnter(null);
        }

        UpdateAllColors();
    }

    /// <summary>
    /// Instantiates all the the room buttons that shows the level grid.
    /// </summary>
    private void DisplayLevelLayout()
    {
        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                _roomButtons[x, y] = Instantiate(_iconRef, gameObject.transform);
                _roomButtons[x, y].name = "(" + x + "," + y + ")";

                int posX = x;
                int posY = y;

                _toggleButtons += isInteractable => SetIsInteractable(posX, posY, isInteractable);
                _roomButtons[x,y].Position = new Vector2(posX, posY);
                _roomButtons[x, y].onClick.AddListener(() => UpdateSelection(posX, posY));
                _roomButtons[x, y].OnButtonSelect += () => AddNodeToPath(posX, posY);
                _roomButtons[x, y].OnButtonSelect += () => _selectedButton = _roomButtons[posX, posY];
                AssignColor(x, y);
            }
        }

        _eventSystem.firstSelectedGameObject = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y].gameObject;
    }

    private void UpdateAllColors()
    {
        foreach (RoomButtonBehaviour button in _roomButtons)
        {
            AssignColor((int)button.Position.x, (int)button.Position.y);
        }
    }

    /// <summary>
    /// Sets the color for each button for easy identification while debugging,
    /// White - Open room
    /// Green - Start room
    /// Red - End room
    /// Black - Inked/Blocked room
    /// </summary>
    /// <param name="x">The x position of the node to assign color to.</param>
    /// <param name="y">The y position of the node to assign color to.</param>
    private void AssignColor(int x, int y)
    {
        RoomButtonBehaviour button = _roomButtons[x, y];

        if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "Start")
        {
            button.image.color = Color.green;
            _level.AddNodeToPlayerPath(new Vector2(x, y), new Vector2(x, y));
        }
        else if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "End")
            button.image.color = Color.yellow;
        else if (_level.RoomGraph.GetNode(x, y).Data.inkColor == "Black")
        {
            button.image.color = Color.black;
            button.enabled = false;
        }
        else if (button.MarkedForRemoval)
            button.image.color = Color.red;
        else if (button.AddedToPath)
            button.image.color = Color.cyan;
        else
            button.image.color = Color.white;
    }

    private void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            EraseActive = !LevelDisplayBehaviour.EraseActive;
            MarkNodesForRemoval();
        }
    }
}
