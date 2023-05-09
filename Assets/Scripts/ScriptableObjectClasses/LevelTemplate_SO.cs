using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    /// <summary>
    /// Object containing data for level generation.
    /// </summary>
    [CreateAssetMenu(menuName = "LevelGeneration/LevelTemplate")]
    public class LevelTemplate_SO : ScriptableObject
    {
        [SerializeField]
        private int _world;
        [SerializeField]
        private int _brownAmount;
        [SerializeField]
        private int _miniBossAmount;
        [SerializeField]
        private int _treasureAmount;
        [SerializeField]
        private int _trapAmount;
        [SerializeField]
        [Range(0, 3)]
        private int _keyAmount;
        [SerializeField]
        private int _width;
        [SerializeField]
        private int _height;
        [SerializeField]
        private int _complexityScore;
        [SerializeField]
        private int _defaultGraphite;

        public int BrownAmount { get => _brownAmount; }
        public int MiniBossAmount { get => _miniBossAmount;  }
        public int TreasureAmount { get => _treasureAmount; }
        public int TrapAmount { get => _trapAmount; }
        public int Width { get => _width; }
        public int Height { get => _height; }
        public int ComplexityScore { get => _complexityScore; }
        public int DefaultGraphite { get => _defaultGraphite; }
        public int World { get => _world; set => _world = value; }
        public int KeyAmount { get => _keyAmount; }
    }
}