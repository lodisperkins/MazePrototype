using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;


public class LevelBehaviour : MonoBehaviour
{
    private Graph<RoomDescription> _roomGraph;
    private int _width;
    private int _height;

    private void Awake()
    {
        _roomGraph = new Graph<RoomDescription>(_width, _height);

    }

    private void GenerateShapes()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
