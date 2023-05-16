using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBehaviour : MonoBehaviour
{
    private PlayerMovementBehaviour _movement;
    private PlayerCombatBehaviour _combat;

    // Start is called before the first frame update
    void Awake()
    {
        _movement = GetComponent<PlayerMovementBehaviour>();
        _combat = GetComponent<PlayerCombatBehaviour>();
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
