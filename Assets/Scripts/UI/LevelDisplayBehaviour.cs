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
    private static bool _drawActive;
    private List<RoomButtonBehaviour> _selectedButtonNeighbors;
    private List<RoomButtonBehaviour> _currentRemovableNodes;

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
    /// Gets all buttons in cardinal directions surrounding the button at the given position.
    /// </summary>
    /// <param name="button">The button to get the neighbors for.</param>
    /// <returns></returns>
    private List<RoomButtonBehaviour> GetButtonNeighbors(RoomButtonBehaviour button)
    {
        int x = (int)button.Position.x;
        int y = (int)button.Position.y;

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
        //Returns if the erase mode is active.
        if (EraseActive)
            return;

        //Return if the path already contains the node.
        if (!_level.PlayerPath.Contains(_level.RoomGraph.GetNode(x, y)))
        {
            _roomButtons[x, y].OnDeselect(null);
            return;
        }

        //Deactivate all other buttons so that the player can only focus on the selected node.
        //FocusActive = true;    
        _selectedButton = _roomButtons[x, y];
        //_toggleButtons?.Invoke(false);

        

        //Make all the neighbors of the selected button interactable.
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
        //Return if the node hasn't been selected to draw from.
        if (!FocusActive)
            return false;

        bool added = false;

        //Disable focus so the player can select a new node to draw from.
        FocusActive = false;
        _toggleButtons?.Invoke(true);

        //Try to add the node to the player path. If successful...
        if (_level.AddNodeToPlayerPath(_selectedButton.Position, new Vector2(x, y)))
        {
            //...mark the node as added.
            _roomButtons[x, y].OnAddedToPath?.Invoke();
            _roomButtons[x, y].AddedToPath = true;

            //If the exit was added to the path allow the player to load the dungeon.
            if (_level.ExitPosition == new Vector2(x, y))
                _loadDungeonButton.SetActive(true);

            added = true;
        }

        //Updates the images for all the neighbors.
        foreach (RoomButtonBehaviour button in _selectedButtonNeighbors)
        {
            button.OnPointerExit(null);
        }

        return added;
    }


    /// <summary>
    /// Trys to remove the room at the given position in the players path.
    /// </summary>
    /// <param name="x">The x position of the room to remove.</param>
    /// <param name="y">The y position of the room to remove.</param>
    public bool RemoveNodeFromPath(int x, int y)
    {
        //Return if the erase mode isn't enabled.
        if (!EraseActive)
            return false;

        bool removed = false;

        RoomButtonBehaviour buttonToRemove = _roomButtons[x, y];

        //Try to remove the node from the player path. If successful...
        if (_level.RemoveNodeFromPlayerPath(new Vector2(x, y)))
        {
            //...mark the node as removed.
            buttonToRemove.OnRemovedFromPath?.Invoke();
            buttonToRemove.AddedToPath = false;
            buttonToRemove.MarkedForRemoval = false;

            //Check if there is a node in the path that connects to the exit and start.
            bool exitNodeHasPath = GetButtonNeighbors((int)_level.ExitPosition.x, (int)_level.ExitPosition.y).
                FindAll(button => button.AddedToPath).Count > 0 && buttonToRemove.Position != _level.ExitPosition;

            bool startNodeHasPath = GetButtonNeighbors((int)_level.StartPosition.x, (int)_level.StartPosition.y).
                FindAll(button => button.AddedToPath).Count > 0;

            //If there isn't a path to the exit or the start...
            if (!exitNodeHasPath || !startNodeHasPath)
                //...prevent the player from loading the dungeon.
                _loadDungeonButton.SetActive(false);

            removed = true;
        }

        //Updates the images for all the neighbors.
        foreach (RoomButtonBehaviour button in _selectedButtonNeighbors)
        {
            button.OnPointerExit(null);
        }

        //Updates the images for all nodes on the grid to show what nodes are now erasable.
        MarkNodesForRemoval();
        return removed;
    }

    /// <summary>
    /// Gets the the nodes on the grid that has one neighbor in the player path or one exit.
    /// </summary>
    /// <returns>A list of the rooms that can be removed.</returns>
    public List<RoomButtonBehaviour> GetRemovableNodes()
    {
        List<RoomButtonBehaviour> removableNodes = new List<RoomButtonBehaviour>();

        //Iterate only through nodes in the player path.
        foreach (Node<RoomDescription> node in _level.PlayerPath)
        {
            //Prevents the player from removing the start node from the path.
            if (node.Position == _level.StartPosition)
                continue;

            RoomButtonBehaviour button = _roomButtons[(int)node.Position.x, (int)node.Position.y];

            List<RoomButtonBehaviour> buttonNeighbors = GetButtonNeighbors(button);

            //The button only has one neighbor added to the path or has one exit...
            if (buttonNeighbors.FindAll(neighbor => neighbor.AddedToPath).Count == 1 || node.Data.ExitCount <= 1)
                //...add to the list of nodes that can be removed.
                removableNodes.Add(button);
        }

        return removableNodes;
    }

    /// <summary>
    /// Removes the mark for removal on all nodes in the current removal list.
    /// </summary>
    private void UnmarkNodesForRemoval()
    {
        //Unmark all the nodes in the list so that they are no longer labeled as removable.
        foreach (RoomButtonBehaviour button in _currentRemovableNodes)
            button.MarkedForRemoval = false;

        //Makes all buttons interactable again.
        _toggleButtons?.Invoke(true);
        _selectedButton.interactable = true;

        //Places the player cursor on the object interacted with.
        _eventSystem.SetSelectedGameObject(_selectedButton.gameObject);
        //Updates all visuals for nodes.
        UpdateAllColors();
    }

    /// <summary>
    /// Sets all nodes with a single edge or exit to be removable and adds them to the current list.
    /// </summary>
    private void MarkNodesForRemoval()
    {
        //Turn off all other buttons so that only the removable nodes can be interacted with.
        _toggleButtons?.Invoke(false);
        _selectedButton.interactable = false;

        //Finds all removable nodes.
        _currentRemovableNodes = GetRemovableNodes();

        //Allow all nodes in the removal list to be interacted with.
        foreach (RoomButtonBehaviour button in _currentRemovableNodes)
        {
            if (button.enabled)
            {
                button.interactable = true;
                button.MarkedForRemoval = true;
            }
        }

        //If there are still nodes that can be removed...
        if (_currentRemovableNodes.Count > 0)
            //...highlight the first node in the list.
            _eventSystem.SetSelectedGameObject(_currentRemovableNodes[0].gameObject);

        UpdateAllColors();
    }

    private void UseNode(int posX, int posY)
    {
        if (EraseActive)
            RemoveNodeFromPath(posX, posY);
        else if (_drawActive)
            AddNodeToPath(posX, posY);
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
                _roomButtons[x, y].OnButtonSelect += () => UseNode(posX, posY);
                AssignColor(x, y);
            }
        }

        _eventSystem.firstSelectedGameObject = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y].gameObject;
    }

    /// <summary>
    /// Updates the visuals for every node in the grid.
    /// </summary>
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
            button.AddedToPath = true;
        }
        else if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "End")
            button.image.color = Color.yellow;
        else if (_level.RoomGraph.GetNode(x, y).Data.inkColor == "Black")
        {
            button.image.color = Color.black;
            button.enabled = false;
        }
        else if (button.MarkedForRemoval)
            button.image.color = Color.red / 2;
        else if (button.AddedToPath)
            button.image.color = Color.cyan;
        else
            button.image.color = Color.white;
    }

    private void Update()
    {
        //Toggle erase mode when player presses cancel button.
        if (Input.GetButton("Cancel"))
        {
            EraseActive = true;
            MarkNodesForRemoval();
        }
        else if (EraseActive)
        {
            EraseActive = false;
            UnmarkNodesForRemoval();
        }


    }
}
