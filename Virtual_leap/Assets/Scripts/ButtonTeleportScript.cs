using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;


public class ButtonTeleportScript : MonoBehaviour
{
    public UnityEvent onPressed, onRealised;

    [SerializeField] private Vector3 closedPos;
    [SerializeField] private Transform door;

    private bool isPressed;
    private Vector3 startPos;
    private ConfigurableJoint joint;

    private float threshold = 0.1f;
    private float deadZone = 0.025f;

    [SerializeField] private Transform player;
    public Transform TeleportGoal;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPressed && GetValue() + threshold >= 1)
        {
            Pressed();
        }
        if (isPressed && GetValue() - threshold <= 0)
        {
            Realesed();
        }
    }

    private void Pressed()
    {
        isPressed = true;
        if (Math.Round(door.position.x, 1) == Math.Round(closedPos.x, 1))
        {
            player.position = TeleportGoal.position;
        }
    }

    private void Realesed()
    {
        isPressed = false;
        onRealised.Invoke();
    }

    private float GetValue()
    {
        var value = Vector3.Distance(startPos, transform.localPosition) / joint.linearLimit.limit;
        if (Math.Abs(value) < deadZone)
        {
            value = 0;
        }
        return Mathf.Clamp(value, -1f, 1f);
    }
}
