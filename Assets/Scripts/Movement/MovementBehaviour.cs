using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    private Rigidbody _rigidBody;
    [SerializeField] 
    private float _movementSpeed;
    [SerializeField]
    private bool _faceMoveDirection = true;
    private Vector3 _moveDirection;

    public Vector3 MoveDirection { get => _moveDirection;  set => _moveDirection = value; }

    public float Speed
    {
        get { return _movementSpeed; }
        set { _movementSpeed = value; }
    }


    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public bool FaceMoveDirection { get => _faceMoveDirection; set => _faceMoveDirection = value; }


    // Start is called before the first frame update
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    public virtual Vector3 GetVelocity()
    {
        return _movementSpeed * MoveDirection;
    }

    public virtual void SetVelocity(Vector3 value)
    {
        _movementSpeed = value.magnitude;
        MoveDirection = value.normalized;
    }

    public virtual void Move()
    {
        Vector3 velocity = MoveDirection * _movementSpeed;
        transform.position += velocity * Time.deltaTime;

        if (MoveDirection.magnitude > 0 && FaceMoveDirection)
            transform.rotation = Quaternion.LookRotation(MoveDirection, transform.up);
    }

    public virtual void Move(Vector3 moveDirection)
    {
        MoveDirection = moveDirection;
        Vector3 velocity = MoveDirection * _movementSpeed;
        transform.position += velocity * Time.deltaTime;

        if (MoveDirection.magnitude > 0 && FaceMoveDirection)
            transform.rotation = Quaternion.LookRotation(MoveDirection, transform.up);
    }
}
