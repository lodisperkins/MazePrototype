using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{

    public enum EntityType
    {
        KEY,
        MERCHANT,
        MONSTER,
        EMPTY
    }

    /// <summary>
    /// Object containing data for tile generation.
    /// </summary>
    [CreateAssetMenu(menuName = "LevelGeneration/EntityDescription")]
    public class EntityDescription : ScriptableObject
    {
        public EntityType Entity;
        public GameObject Visual;
    }
}