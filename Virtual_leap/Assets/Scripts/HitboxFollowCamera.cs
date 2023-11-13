using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxFollowCamera : MonoBehaviour
{
    [SerializeField] private GameObject camera;

    void FixedUpdate()
    {
        this.gameObject.transform.position = new Vector3(camera.transform.position.x, this.gameObject.transform.position.y, camera.transform.position.z);
    }
}
