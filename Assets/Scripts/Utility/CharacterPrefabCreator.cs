using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class CharacterPrefabCreator
{
    /// <summary>
    /// Creates a new variant of the base character prefab and puts it in its own folder.
    /// </summary>
    [MenuItem("Assets/Create/Character", false, 5)]
    public static void CreateBaseCharacterVariant()
    {
        Object characterBasePrefab = AssetDatabase.LoadAssetAtPath<Object>("Assets/Characters/CharacterBase.prefab");
        GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(characterBasePrefab);
        AssetDatabase.CreateFolder("Assets/Characters", "NewCharacter");
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, "Assets/Characters/NewCharacter/NewCharacter.prefab");
    }
}

#endif