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
    public enum InkColor
    {
        NONE,
        BLACK
    }
    
    public enum StickerType
    {
        NONE,
        START,
        KEY,
        EXIT
    }

    public struct RoomDescription
    {
        private bool _hasNorthExit;
        private bool _hasSouthExit;
        private bool _hasEastExit;
        private bool _hasWestExit;
        private int _exitCount;
         
        public InkColor InkColor { get; set; }
        public StickerType StickerType { get; set; }
        public bool HasNorthExit 
        {
            get => _hasNorthExit;
            
            set
            {
                if (value && !_hasNorthExit)
                    ExitCount++;
                else if (!value && _hasNorthExit)
                    ExitCount--;

                _hasNorthExit = value;
            }
        }
        public bool HasSouthExit
        {
            get => _hasSouthExit;

            set
            {
                if (value && !_hasSouthExit)
                    ExitCount++;
                else if (!value && _hasSouthExit)
                    ExitCount--;

                _hasSouthExit = value;
            }
        }
        public bool HasEastExit
        {
            get => _hasEastExit;

            set
            {

                if (value && !_hasEastExit)
                    ExitCount++;
                else if (!value && _hasEastExit)
                    ExitCount--;

                _hasEastExit = value;
            }
        }
        public bool HasWestExit
        {
            get => _hasWestExit;

            set
            {

                if (value && !_hasWestExit)
                    ExitCount++;
                else if (!value && _hasWestExit)
                    ExitCount--;

                _hasWestExit = value;
            }
        }

        public int ExitCount 
        {
            get => _exitCount;
            set
            {
                _exitCount = value;

                if (_exitCount < 0)
                    _exitCount = 0;
            }
        }
    }
    
    public class RoomBehaviour : MonoBehaviour
    {
        private TileDescription[] _tileDescriptionReferences;
        private EntityDescription[] _entityDescriptionReferences;
        private TileDescription[,] _tileDescriptions;
        private RoomData _data;
        private int _world;
        private Vector3 _spawnPosition;

        public int Width { get => _tileDescriptions.GetLength(1); }
        public int Height { get => _tileDescriptions.GetLength(0); }
        public int World { get => _world; set => _world = value; }

        /// <summary>
        /// Instantiates a new room that loads with the given description.
        /// </summary>
        /// <param name="world">The world the room is being generated in.</param>
        /// <param name="parentLevel">The level that this room is a part of.</param>
        /// <param name="description">Data containing details about the room.</param>
        /// <returns>The newly instantiated room.</returns>
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

        /// <summary>
        /// Instantiates each tile based on the tiles in the room template.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the ID of the tile hasn't been defined.</exception>
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
                            throw new ArgumentOutOfRangeException("There is no enumerator value that matches the tile ID loaded. Tile ID was " + (TileID)_data.layers[0].data2D[y, x]);
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates each entity based on the entities in the room template.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the ID of the tile hasn't been defined.</exception>
        private void SpawnEntity(Vector2 gridPosition, Vector3 position)
        {
            Entity entityData = _data.layers[1].entities.FirstOrDefault(e => new Vector2(e.x, e.y) == gridPosition);

            if (entityData.Equals(default(Entity)))
                return;

            GameObject entityVisual = _entityDescriptionReferences.First(e => e.Entity.ToString() == entityData.name.ToUpper()).Visual;

            GameObject entity = Instantiate(entityVisual, gameObject.transform);
            entity.transform.position = position;   
        }
        
        /// <summary>
        /// Checks to see if there should be a an exit at the given position.
        /// Where a door should spawn is based on how the path is drawn and generated in the LevelBehaviour script.
        /// </summary>
        /// <param name="x">The x position of the door tile.</param>
        /// <param name="y">The y position of the door tile.</param>
        /// <returns></returns>
        private bool CheckDoorSpawn(int x, int y)
        {
            if (x == Width - 1 && _data.Description.HasEastExit)
                return true;
            else if (x == 0 && _data.Description.HasWestExit)
                return true;
            else if (y == Height - 1 && _data.Description.HasNorthExit)
                return true;
            else if (y == 0 && _data.Description.HasSouthExit)
                return true;

            return false;

        }

        /// <summary>
        /// Grabs all files in the room template folder and picks one to load at random.
        /// </summary>
        /// <param name="description"></param>
        private void LoadRoomData(RoomDescription description)
        {
            _tileDescriptionReferences = Resources.LoadAll<TileDescription>("World" + World + "/TileDescriptions");
            _entityDescriptionReferences = Resources.LoadAll<EntityDescription>("World" + World + "/EntityDescriptions");

            string sticker = "";

            if (description.StickerType == StickerType.START)
                sticker = "NONE";
            else
                sticker = description.StickerType.ToString();

            string[] files = Directory.GetFiles("Assets/Resources/World" + World + "/RoomTemplates", "*" + sticker + ".json");

            int roomNum = Random.Range(0, files.Length);

            StreamReader reader = new StreamReader(files[roomNum]);
            string dat = reader.ReadToEnd();

            _data = JsonConvert.DeserializeObject<RoomData>(dat);
            _data.Description = description;

            _tileDescriptions = new TileDescription[_data.layers[0].data2D.GetLength(0), _data.layers[0].data2D.GetLength(1)];
            InitializeTiles();
        }

        /// <summary>
        /// Instantiates each room tile and ensures they are evenly spaced.
        /// </summary>
        /// <param name="startingPosition">The position in the world to start spawning tiles.</param>
        public void BuildRoom(Vector3 startingPosition)
        {

            _spawnPosition = startingPosition;

            for (int y = 0; y < _tileDescriptions.GetLength(0); y++)
            {
                _spawnPosition.x = startingPosition.x;
                for (int x = 0; x < _tileDescriptions.GetLength(1); x++)
                {
                    GameObject visual = _tileDescriptions[y, x]?.Visual;

                    if (visual)
                    {
                        GameObject tile = Instantiate(visual, gameObject.transform);
                        tile.transform.position = _spawnPosition;

                        SpawnEntity(new Vector2(x * _data.layers[0].gridCellWidth, y * _data.layers[0].gridCellHeight), tile.transform.position);
                    }
                    _spawnPosition.x++;
                }

                _spawnPosition.z++;
            }

        }
    }

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
        public Entity[] entities { get; set; }
    }

    public struct Entity
    {
        public string name { get; set; }
        public int id { get; set; }
        public string _eid { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int originX { get; set; }
        public int originY { get; set; }
    }

    public struct RoomData
    {
        public string ogmoVersion { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public RoomDescription Description { get; set; }
        public Layer[] layers { get; set; }
    }
}