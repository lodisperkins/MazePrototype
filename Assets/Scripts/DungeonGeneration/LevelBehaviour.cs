using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;

public class LevelBehaviour : MonoBehaviour
{
    private RoomBehaviour _room;

    // Start is called before the first frame update
    void Start()
    {
        _room = GetComponent<RoomBehaviour>();
        _room.LoadRoomData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
