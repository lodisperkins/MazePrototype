using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayBehaviour : MonoBehaviour
{
    [SerializeField] private LevelBehaviour _level;
    [SerializeField] private Button _iconRef;
    private Button[,] _roomButtons;
    private Button _selectedButton;

    // Start is called before the first frame update
    void Start()
    {
        _roomButtons = new Button[_level.Width, _level.Height];
        DisplayLevelLayout();
    }

    public void UpdateSelection(int x, int y)
    {
        _selectedButton = _roomButtons[x, y];

        _selectedButton.FindSelectableOnDown()?.Select();
        _selectedButton.FindSelectableOnDown().interactable = true;

        _selectedButton.FindSelectableOnLeft()?.Select();   
       
        _selectedButton.FindSelectableOnRight()?.Select(); 
        _selectedButton.FindSelectableOnUp()?.Select();
    }

    void DisplayLevelLayout()
    {
        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                _roomButtons[x, y] = Instantiate(_iconRef, gameObject.transform);
                _roomButtons[x, y].name = "(" + x + "," + y + ")";
                _roomButtons[x, y].onClick.AddListener(() => UpdateSelection(x, y));
                _roomButtons[x, y].onClick.AddListener(() => _level.AddNodeToPlayerPath(x, y));
                AssignColor(x, y);
            }
        }
    }

    private void AssignColor(int x, int y)
    {
        if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "Start")
        {
            _roomButtons[x, y].image.color = Color.green;
            UpdateSelection(x, y);
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
