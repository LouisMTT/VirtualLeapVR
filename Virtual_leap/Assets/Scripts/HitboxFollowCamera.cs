using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxFollowCamera : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    public Timer timer;
    [SerializeField] private Transform player;
    public Transform TeleportGoal;

    void FixedUpdate()
    {
        // location
        this.gameObject.transform.position = new Vector3(camera.transform.position.x, this.gameObject.transform.position.y, camera.transform.position.z);

        // rotation
        this.gameObject.transform.localRotation = new Quaternion(this.gameObject.transform.localRotation.x, camera.transform.localRotation.y, 
            this.gameObject.transform.localRotation.z, this.gameObject.transform.localRotation.w);
    }
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Start"){
            timer.ChangeStatus(true);
        }
        if(other.tag == "Finish"){
            timer.ChangeStatus(false);
        }
        if(other.tag == "Death"){
            player.transform.position = TeleportGoal.transform.position;
        }
        if(other.tag == "Respawn"){
            TeleportGoal = other.gameObject.transform;
        }
        if(other.tag == "Reset"){
            timer.Reset();
        }
    }

}
