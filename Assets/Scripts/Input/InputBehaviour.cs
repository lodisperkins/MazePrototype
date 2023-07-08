using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBehaviour : MonoBehaviour
{
    private MovementBehaviour _movement;
    private CombatBehaviour _combat;

    // Start is called before the first frame update
    void Awake()
    {
        _movement = GetComponent<MovementBehaviour>();
        _combat = GetComponent<CombatBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        _movement.Move(moveDirection);

        if (Input.GetButtonDown("Fire1"))
            _combat.UseAbility1();
    }
}
