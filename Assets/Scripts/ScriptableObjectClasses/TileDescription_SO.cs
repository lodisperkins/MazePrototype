using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public enum FloorType
    {
        FLOOR,
        WALL,
        PIT,
        SPIKES,
        FOUNTAIN,
        TREASURE
    }

    public enum TileID
    {
        FLOOR = -1,
        VOID = 509,
        WALL = 482,
        POSSIBLEWALL = 3237,
        DOOR = 197,
    }

    /// <summary>
    /// Object containing data for tile generation.
    /// </summary>
    [CreateAssetMenu(menuName = "LevelGeneration/TileDescription")]
    public class TileDescription_SO : ScriptableObject
    {
        public FloorType Floor;
        public GameObject Visual;
    }
}