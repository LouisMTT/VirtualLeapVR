using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ButtonScript : MonoBehaviour
{
    public Music music;
    [SerializeField] private int FloorNr;
    public UnityEvent onPressed, onRealised;

    private bool isPressed;
    private Vector3 startPos;
    private ConfigurableJoint joint;

    private float threshold = 0.1f;
    private float deadZone = 0.025f;

    //left
    [SerializeField] private Transform doorL;
    [SerializeField] private Vector3 closedPosL;
    [SerializeField] private Vector3 openedPosL;

    //right
    [SerializeField] private Transform doorR;

    [SerializeField] private int state;

    


    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPressed && GetValue() + threshold >= 1)
        {
            Pressed();
        }
        if(isPressed && GetValue() - threshold <= 0)
        {
            Realesed();
        }

        if (state == 1 && doorL.position.x > openedPosL.x)
        {
            Open();
            music.ChangeTrack(FloorNr);
        }
        else if (state == 0 && doorL.position.x < closedPosL.x)
        {
            Close();
            music.ChangeTrack(0);
        }
    }

    private void Pressed()
    {
        isPressed = true;
        if (state == 0)
        {
            state = 1;
        }
        else if (state == 1)
        {
            state = 0;
        }
        Debug.Log("Perssed");
    }

    private void Realesed()
    {
        isPressed = false;
        onRealised.Invoke();
    }

    private float GetValue()
    {
        var value = Vector3.Distance(startPos, transform.localPosition) / joint.linearLimit.limit;
        if(Math.Abs(value) < deadZone)
        {
            value = 0;
        }
        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Open()
    {
        if (state == 1 && doorL.position.x > openedPosL.x)
        {
            doorL.position = new Vector3(doorL.position.x + Time.deltaTime * -1 * 1.0f, doorL.position.y, doorL.position.z);
            doorR.position = new Vector3(doorR.position.x + Time.deltaTime * 1 * 1.0f, doorR.position.y, doorR.position.z);
        }
    }

    private void Close()
    {
        if (state == 0 && doorL.position.x < closedPosL.x)
        {
            doorL.position = new Vector3(doorL.position.x + Time.deltaTime * 1 * 1.0f, doorL.position.y, doorL.position.z);
            doorR.position = new Vector3(doorR.position.x + Time.deltaTime * -1 * 1.0f, doorR.position.y, doorR.position.z);
        }
    }
}
