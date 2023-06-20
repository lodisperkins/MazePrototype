using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnerBehaviour : MonoBehaviour
{
    [SerializeField] private MovementBehaviour _playerReference;
    private static MovementBehaviour _player;

    public static MovementBehaviour Player { get => _player; }
    
    public void SpawnPlayer(LevelBehaviour level)
    {
        _player = Instantiate(_playerReference, level.PlayerSpawnPosition, new Quaternion());
    }
}
