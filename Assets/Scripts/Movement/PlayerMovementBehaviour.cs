using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementBehaviour : MonoBehaviour
{
    private Rigidbody _rigidBody;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private int _keysCollected;
    private Vector3 _moveDirection;

    public Vector3 MoveDirection { get => _moveDirection;  set => _moveDirection = value; }

    public float CurrentSpeed
    {
        get { return (_moveDirection * _movementSpeed).magnitude; }
    }


    // Start is called before the first frame update
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            _keysCollected++;
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Exit") && _keysCollected == LevelBehaviour.CurrentKeyRequirement)
            Debug.Log("Level exit success!");

    }

    private void FixedUpdate()
    {
        Vector3 velocity = MoveDirection * _movementSpeed;
        transform.position += velocity * Time.deltaTime;

        if (MoveDirection.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(MoveDirection, transform.up);
    }
}
