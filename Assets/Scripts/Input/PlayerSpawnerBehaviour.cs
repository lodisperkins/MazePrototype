using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnerBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerMovementBehaviour _playerReference;
    private static PlayerMovementBehaviour _player;

    public static PlayerMovementBehaviour Player { get => _player; }
    
    public void SpawnPlayer(LevelBehaviour level)
    {
        _player = Instantiate(_playerReference, level.PlayerSpawnPosition, new Quaternion());
    }
}
