using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMovementBehaviour : MonoBehaviour
{
    [SerializeField] private float _height;

    // Update is called once per frame
    void Update()
    {
        if (PlayerSpawnerBehaviour.Player)
            transform.position = PlayerSpawnerBehaviour.Player.transform.position + Vector3.up * _height;
    }
}
