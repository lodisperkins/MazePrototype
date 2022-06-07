using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using System.IO;

namespace DungeonGeneration
{
    public class RoomBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TileDescription[] _tileDescriptionReferences;
        private TileDescription[,] _tileDescriptions;
        private int _exits;

        public void LoadRoomData()
        {
            string[] files = Directory.GetFiles("Assets/Resources/RoomTemplates", "*.json");
            bool success = File.Exists(files[0]);
        }
    }
}