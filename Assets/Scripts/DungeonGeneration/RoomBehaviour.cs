using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using File = UnityEngine.Windows.File;

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
        [SerializeField]
        private TileDescription[] _tileDescriptionReferences;
        private TileDescription[,] _tileDescriptions;
        private int _exits;
        private RoomData _data;

        private void InitializeTiles()
        {
            for (int i = 0; i < _data.height; i++)
            {
                for (int j = 0; j < _data.width; j++)
                {
                    _tileDescriptions[j, i] = Array.Find(_tileDescriptionReferences,
                        description => description.Floor == (FloorType)_data.layers[0].data2D[j, i]);
                }
            }
        }
        
        public void LoadRoomData()
        {
            string[] files = Directory.GetFiles("Assets/Resources/RoomTemplates", "*.json");
            StreamReader reader = new StreamReader(files[0]);
            string dat = reader.ReadToEnd();
            _data = JsonConvert.DeserializeObject<RoomData>(dat);
            InitializeTiles();
        }
    }
}