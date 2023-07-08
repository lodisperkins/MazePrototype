using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMovementBehaviour : MonoBehaviour
{
    [SerializeField]
    private Vector3 _offset = Vector3.up;

    // Update is called once per frame
    void Update()
    {
        if (!PlayerSpawnerBehaviour.Player)
            return;

        transform.position = PlayerSpawnerBehaviour.Player.transform.position + _offset;

        transform.LookAt(PlayerSpawnerBehaviour.Player.transform);
    }
}
