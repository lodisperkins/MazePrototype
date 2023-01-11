using DungeonGeneration;
using System;
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
    private static bool _eraseActive;
    private static bool _drawActive;
    private List<RoomButtonBehaviour> _selectedButtonNeighbors;
    private List<RoomButtonBehaviour> _currentRemovableNodes;
    private int _currentKeyCount;

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
    public static bool DrawActive { get => _drawActive; set => _drawActive = value; }

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
        _selectedButton = _roomButtons[x, y];

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
        if (!_drawActive)
            return false;

        bool added = false;

        //If the cursor isn't over a room that's in the path.
        if (_level.PlayerPath.Find(node => node.Position == _selectedButton.Position) == null)
            return false;

        //Try to add the node to the player path. If successful...
        if (_level.AddNodeToPlayerPath(_selectedButton.Position, new Vector2(x, y)))
        {
            //...mark the node as added.
            _roomButtons[x, y].OnAddedToPath?.Invoke();
            _roomButtons[x, y].AddedToPath = true;


            if (Array.Exists(_level.KeyPositions, value => value == new Vector2(x,y)))
                _currentKeyCount++;


            //If the exit was added to the path allow the player to load the dungeon.
            if (_level.PlayerPath.Find(node => node.Position == _level.ExitPosition) != null && _currentKeyCount == _level.Template.KeyAmount)
                _loadDungeonButton.SetActive(true);

            added = true;
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

        if (!buttonToRemove.MarkedForRemoval)
            return false;

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

            if (Array.Exists(_level.KeyPositions, value => value == new Vector2(x, y)))
                _currentKeyCount--;

            //If there isn't a path to the exit or the start...
            if (!exitNodeHasPath || !startNodeHasPath || _currentKeyCount != _level.Template.KeyAmount)
                //...prevent the player from loading the dungeon.
                _loadDungeonButton.SetActive(false);

            removed = true;
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

            //The button only has one neighbor added to the path or has one exit...
            if (node.Data.ExitCount <= 1 || (node.Position == _level.ExitPosition && node.Data.ExitCount <= 2))
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
        if (_currentRemovableNodes == null || _currentRemovableNodes.Count == 0)
            return;

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

        UpdateAllColors();
    }

    /// <summary>
    /// Adds or removes a node based on the current mode that's active.
    /// </summary>
    /// <param name="posX">The x position on the graph for the node to remove.</param>
    /// <param name="posY">The y position on the graph for the node to remove.</param>
    private void UseNode(int posX, int posY)
    {
        //Return if the node is an ink blot.
        if (_selectedButton.image.color == Color.black)
            return;

        //Remove or add based on the current mode active.
        if (EraseActive)
            RemoveNodeFromPath((int)_selectedButton.Position.x, (int)_selectedButton.Position.y);
        else if (_drawActive)
            AddNodeToPath(posX, posY);

        //Update the button that is selected after since adding and removing needs to know about the previous selection.
        _selectedButton = _roomButtons[posX, posY];
    }

    /// <summary>
    /// Instantiates all the the room buttons that shows the level grid.
    /// </summary>
    private void DisplayLevelLayout()
    {
        //Loop through level grid to spawn buttons.
        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                //Spawn a new button with a name that matches its coordinate.
                _roomButtons[x, y] = Instantiate(_iconRef, gameObject.transform);
                RoomButtonBehaviour currentButton = _roomButtons[x, y];
                currentButton.name = "(" + x + "," + y + ")";

                //Two temporary variables are created to be used with delegates.
                //This is because variables declared inside of loops can't keep their values when passed as delegate parameters.
                int posX = x;
                int posY = y;
                
                _toggleButtons += isInteractable => SetIsInteractable(posX, posY, isInteractable);

                //Initialize default button values.
                currentButton.Position = new Vector2(posX, posY);
                currentButton.OnButtonSelect += () => UseNode(posX, posY);
                AssignColor(x, y);

                //Manually sets up navigation connections between buttons.
                //Unity connects all buttons to each other automatically which
                //causes unintentional behaviour.
                if (x > 0)
                { 
                    // west connection

                    RoomButtonBehaviour other = _roomButtons[x - 1, y];

                    //Create a new navigation object with custom options for the current button.
                    Navigation currentButtonNavigation = currentButton.navigation;
                    currentButtonNavigation.mode = Navigation.Mode.Explicit;

                    currentButtonNavigation.selectOnLeft = other;
                    currentButton.navigation = currentButtonNavigation;

                    //Create a new navigation object with custom options for the west button.
                    Navigation otherNavigation = other.navigation;
                    otherNavigation.mode = Navigation.Mode.Explicit;

                    otherNavigation.selectOnRight = currentButton;
                    other.navigation = otherNavigation;
                }
                if (y > 0)
                { 
                    // north connection

                    RoomButtonBehaviour other = _roomButtons[x, y - 1];

                    //Create a new navigation object with custom options for the current button.
                    Navigation currentButtonNavigation = currentButton.navigation;
                    currentButtonNavigation.mode = Navigation.Mode.Explicit;

                    currentButtonNavigation.selectOnDown = other;
                    currentButton.navigation = currentButtonNavigation;

                    //Create a new navigation object with custom options for the north button.
                    Navigation otherNavigation = other.navigation;
                    otherNavigation.mode = Navigation.Mode.Explicit;

                    otherNavigation.selectOnUp = currentButton;
                    other.navigation = otherNavigation;
                }
            }
        }

        //Sets the first room as the first selected UI item.
        _eventSystem.firstSelectedGameObject = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y].gameObject;
        _selectedButton = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y];
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

        Node<RoomDescription> node = _level.RoomGraph.GetNode(x, y);


        if (button.MarkedForRemoval)
            button.image.color = Color.red / 2;
        else if (node.Data.StickerType == StickerType.START)
        {
            button.image.color = Color.green;
            _level.AddNodeToPlayerPath(new Vector2(x, y), new Vector2(x, y));
            button.AddedToPath = true;
        }
        else if (node.Data.StickerType == StickerType.EXIT)
            button.image.color = Color.yellow;
        else if (node.Data.InkColor == InkColor.BLACK)
        {
            button.image.color = Color.black;
            button.enabled = false;
        }
        else if (node.Data.StickerType == StickerType.KEY)
            button.image.color = Color.blue;
        else if (button.AddedToPath)
            button.image.color = Color.cyan;
        else
            button.image.color = Color.white;
    }

    private void Update()
    {
        //Toggle erase mode when player presses cancel button.
        if (Input.GetButtonDown("Cancel"))
        {
            EraseActive = true;
            MarkNodesForRemoval();
        }
        else if ((EraseActive && Input.GetButtonUp("Cancel")) || _drawActive)
        {
            EraseActive = false;
            UnmarkNodesForRemoval();
        }

        _drawActive = Input.GetButton("Submit");
    }
}
