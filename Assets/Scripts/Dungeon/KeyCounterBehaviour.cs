using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCounterBehaviour : MonoBehaviour
{
    [SerializeField] private int _keysCollected;


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
}
