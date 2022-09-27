using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementBehaviour : MonoBehaviour
{
    private CharacterController _characterController;
    [SerializeField] private float _movementSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector3 direction)
    {
        _characterController.Move(direction * _movementSpeed * Time.deltaTime);    
    }
}
