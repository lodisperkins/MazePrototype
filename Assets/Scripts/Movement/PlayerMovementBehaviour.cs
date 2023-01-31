using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementBehaviour : MonoBehaviour
{
    private Rigidbody _rigidBody;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private int _keysCollected;

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

    public void Move(Vector3 direction)
    {
        _rigidBody.MovePosition(transform.position + direction * _movementSpeed * Time.deltaTime);    
    }
}
