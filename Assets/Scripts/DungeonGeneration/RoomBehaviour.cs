using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using File = UnityEngine.Windows.File;
using Random = UnityEngine.Random;

namespace DungeonGeneration
{
    public struct Layer
    {
        public string name { get; set; }
        public string _eid { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public int gridCellWidth { get; set; }
        public int gridCellHeight { get; set; }
        public int gridCellsX { get; set; }
        public int gridCellsY { get; set; }
        public string tileset { get; set; }
        public int[,] data2D { get; set; }
        public int exportMode { get; set; }
        public int arrayMode { get; set; }
    }

    public struct RoomData
    {
        public string ogmoVersion { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public RoomDescription values { get; set; }
        public Layer[] layers { get; set; }
    }

    public struct RoomDescription
    {
        public string inkColor { get; set; }
        public string stickerType { get; set; }
        public bool hasNorthExit { get; set; }
        public bool hasSouthExit { get; set; }
        public bool hasEastExit { get; set; }
        public bool hasWestExit { get; set; }
    }
    
    public class RoomBehaviour : MonoBehaviour
    {
        private TileDescription[] _tileDescriptionReferences;
        private TileDescription[,] _tileDescriptions;
        private int _exits;
        private RoomData _data;
        private int _world;


        public int Width { get => _tileDescriptions.GetLength(1); }
        public int Height { get => _tileDescriptions.GetLength(0); }
        public int World { get => _world; set => _world = value; }


        public static RoomBehaviour MakeRoom(int world, Transform parentLevel, RoomDescription description)
        {
            GameObject room = new GameObject("Room");
            room.transform.parent = parentLevel;
            room.transform.position = Vector3.zero;

            RoomBehaviour roomBehaviour = room.AddComponent<RoomBehaviour>();
            roomBehaviour.World = world;
            roomBehaviour.LoadRoomData(description);

            return roomBehaviour;
        }

        private void InitializeTiles()
        {
            for (int y = 0; y < _tileDescriptions.GetLength(0); y++)
            {
                for (int x = 0; x < _tileDescriptions.GetLength(1); x++)
                {
                    TileID tileID = (TileID)_data.layers[0].data2D[y, x];
                    
                    switch (tileID)
                    {
                        case TileID.FLOOR:
                            _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                description => description.Floor == FloorType.FLOOR);
                            break;
                        case TileID.VOID:
                            _tileDescriptions[y, x] = null;
                            break;
                        case TileID.WALL:
                            _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                description => description.Floor == FloorType.WALL);
                            break;
                        case TileID.POSSIBLEWALL:
                            if (Random.Range(0, 2) == 0)
                            {
                                _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                    description => description.Floor == FloorType.FLOOR);
                                break;
                            }
                            
                            _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                description => description.Floor == FloorType.WALL);
                            break;

                        case TileID.DOOR:
                            if (CheckDoorSpawn(x, y))
                            {
                                _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                    description => description.Floor == FloorType.FLOOR);
                                break;
                            }

                            _tileDescriptions[y, x] = Array.Find(_tileDescriptionReferences,
                                description => description.Floor == FloorType.WALL);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        
        private bool CheckDoorSpawn(int x, int y)
        {
            if (x == Width - 1 && _data.values.hasEastExit)
                return true;
            else if (x == 0 && _data.values.hasWestExit)
                return true;
            else if (y == Height - 1 && _data.values.hasNorthExit)
                return true;
            else if (y == 0 && _data.values.hasSouthExit)
                return true;

            return false;

        }

        private void LoadRoomData(RoomDescription description)
        {
            _tileDescriptionReferences = Resources.LoadAll<TileDescription>("World" + World + "/TileDescriptions");
            string[] files = Directory.GetFiles("Assets/Resources/World" + World + "/RoomTemplates", "*.json");

            int roomNum = Random.Range(0, files.Length);

            StreamReader reader = new StreamReader(files[roomNum]);
            string dat = reader.ReadToEnd();

            _data = JsonConvert.DeserializeObject<RoomData>(dat);
            _data.values = description;

            _tileDescriptions = new TileDescription[_data.layers[0].data2D.GetLength(1), _data.layers[0].data2D.GetLength(0)];
            InitializeTiles();
        }

        public void BuildRoom(Vector3 startingPosition)
        {

            Vector3 spawnPosition = startingPosition;

            for (int x = 0; x < _tileDescriptions.GetLength(0); x++)
            {
                spawnPosition.x = startingPosition.x;
                for (int y = 0; y < _tileDescriptions.GetLength(1); y++)
                {
                    GameObject visual = _tileDescriptions[y, x]?.Visual;

                    if (visual)
                    {
                        GameObject tile = Instantiate(visual, gameObject.transform);
                        tile.transform.position = spawnPosition;
                    }

                    spawnPosition.x++;
                }

                spawnPosition.z++;
            }

        }
    }
}