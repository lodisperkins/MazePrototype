using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayBehaviour : MonoBehaviour
{
    [SerializeField] private LevelBehaviour _level;
    [SerializeField] private Image _iconRef;
    [SerializeField] private Image[] _images;

    // Start is called before the first frame update
    void Start()
    {
        _images = new Image[_level.Width * _level.Height];
        DisplayLevelLayout();
    }


    void DisplayLevelLayout()
    {
        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                _images[y + x] = Instantiate(_iconRef, gameObject.transform);
                _images[y + x].name = "(" + x + "," + y + ")";
                AssignColor(x, y);
            }
        }
    }

    private void AssignColor(int x, int y)
    {
        if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "Start")
            _images[y + x].color = Color.green;
        else if (_level.RoomGraph.GetNode(x, y).Data.stickerType == "End")
            _images[y + x].color = Color.red;
        else if (_level.RoomGraph.GetNode(x, y).Data.inkColor == "Black")
            _images[y + x].color = Color.black;
        else
            _images[y + x].color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
