using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelDisplayBehaviour : MonoBehaviour
{
    [SerializeField] private LevelBehaviour _level;
    [SerializeField] private RoomButtonBehaviour _iconRef;
    [SerializeField] private EventSystem eventSystem;
    private RoomButtonBehaviour[,] _roomButtons;
    private RoomButtonBehaviour _selectedButton;

    // Start is called before the first frame update
    void Start()
    {
        _roomButtons = new RoomButtonBehaviour[_level.Width, _level.Height];
        DisplayLevelLayout();
    }

    List<Button> GetButtonNeighbors(int x, int y)
    {
        List<Button> buttons = new List<Button>();

        if (x > 0)
            buttons.Add(_roomButtons[x - 1, y]);
        if (x < _level.Width)
            buttons.Add(_roomButtons[x + 1, y]);
        if (y > 0)
            buttons.Add(_roomButtons[x, y - 1]);
        if (y < _level.Height)
            buttons.Add(_roomButtons[x, y + 1]);

        return buttons;
    }

    public void UpdateSelection(int x, int y)
    {
        _selectedButton = _roomButtons[x, y];

        _selectedButton.interactable = true;

        List<Button> buttons = GetButtonNeighbors(x, y);

        foreach (Button button in buttons)
        {
            button.OnPointerEnter(null);
        }
    }

    void DisplayLevelLayout()
    {
        for (int y = 0; y < _level.Height - 1; y++)
        {
            for (int x = 0; x < _level.Width - 1; x++)
            {
                _roomButtons[x, y] = Instantiate(_iconRef, gameObject.transform);
                _roomButtons[x, y].name = "(" + x + "," + y + ")";
                _roomButtons[x, y].OnButtonSelect += () => UpdateSelection(x, y);
                _roomButtons[x, y].OnButtonSelect += () => _level.AddNodeToPlayerPath(x, y);
                AssignColor(x, y);
            }
        }

        eventSystem.firstSelectedGameObject = _roomButtons[(int)_level.StartPosition.x, (int)_level.StartPosition.y].gameObject;
    }

    private void AssignColor(int x, int y)
    {
        if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "Start")
        {
            _roomButtons[x, y].image.color = Color.green;
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
