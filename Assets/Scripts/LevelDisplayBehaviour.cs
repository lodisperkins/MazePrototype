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
        for (int i = 0; i < _level.Height; i++)
        {
            for (int j = 0; j < _level.Width; j++)
            {
                _images[i + j] = Instantiate(_iconRef, gameObject.transform);
                _images[i + j].name = "(" + j + "," + i + ")";
                _images[i + j].color = _level.RoomGraph.GetNode(j, i).Data.inkColor == "Black" ? Color.black : Color.white;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
