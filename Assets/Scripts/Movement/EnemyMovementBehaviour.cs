using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementBehaviour : MovementBehaviour
{
    private NavMeshAgent _agent;
    [SerializeField]
    private float _detectionRadius;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Move()
    {
        if (PlayerSpawnerBehaviour.Player == null || !PlayerSpawnerBehaviour.Player.gameObject.activeInHierarchy)
            return;

        Vector3 playerPosition = PlayerSpawnerBehaviour.Player.Position;

        float distance = Vector3.Distance(playerPosition, Position);

        if (distance > _detectionRadius)
            return;

        if (!FaceMoveDirection)
            _agent.angularSpeed = 0;

        _agent.speed = Speed;
        _agent.SetDestination(playerPosition);
    }

    public override void Move(Vector3 moveDirection)
    {
        if (!FaceMoveDirection)
            _agent.angularSpeed = 0;

        _agent.speed = Speed;
        _agent.Move(moveDirection);
    }

    public override Vector3 GetVelocity()
    {
        return _agent.velocity;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        MoveDirection = _agent.velocity.normalized;
    }
}
