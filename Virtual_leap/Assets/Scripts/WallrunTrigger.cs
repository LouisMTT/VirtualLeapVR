using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunTrigger : MonoBehaviour
{
    [SerializeField] PlayerMovement player;
    [SerializeField] string side;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Wallrun"))
        {
            Debug.Log("Wallrun check initiated");
            player.wallToRun = other.gameObject;
            if (side.Equals("left"))
            {
                player.wallrunCheckLeft = true;
            }
            else if (side.Equals("right"))
            {
                player.wallrunCheckRight = true;
            }
            else
            {
                Debug.Log("Sides are not set for wallrun script!!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wallrun"))
        {
            Debug.Log("Wallrun check ended");
            player.wallToRun = null;
            player.wallrunCheckLeft = false;
            player.wallrunCheckRight = false;
        }
    }
}
