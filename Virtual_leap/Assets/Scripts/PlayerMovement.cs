using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Threading;
using System;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public XRNode inputSource;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private GameObject groundCheck;
    [SerializeField] private float wallAccel = 2f;
    [SerializeField] private float wallrunTime = 3f;
    [SerializeField] private float aerialAccel = 0.5f;
    [SerializeField] private float wallrunCooldown = 1f;

    private Vector2 inputAxis;
    private XROrigin rig;
    private Rigidbody rb;
    public bool wallrunCheckLeft;
    public bool wallrunCheckRight;
    private bool isWallrunning;
    public GameObject wallToRun;
    private float wallrunTimer;
    private String wallrunSide;

    private bool isGrounded => Physics.OverlapSphere(groundCheck.transform.position, .25f, groundLayer).Length > 0;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<XROrigin>();
        rb = GetComponent<Rigidbody>();
        jumpActionReference.action.performed += OnJump;
        wallrunCheckLeft = false;
        wallrunCheckRight = false;
        isWallrunning = false;
        wallToRun = null;
        wallrunTimer = 0f;
        wallrunSide = "none";
    }

    // Update is called once per frame
    void Update()
    {
        // input processing
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out inputAxis);
    }

    void FixedUpdate()
    {
        // head direction calculations
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);

        // movement on ground/air
        if (!isWallrunning)
        {
            if (isGrounded && direction.magnitude != 0 && Mathf.Abs(rb.velocity.magnitude) < maxSpeed)
            {
                rb.velocity += direction * acceleration;
            }
            else if (!isGrounded && direction.magnitude != 0 && Mathf.Abs(rb.velocity.magnitude) < maxSpeed)
            {
                rb.velocity += direction * aerialAccel;
            }
        }

        // wallrun initialization logic
        if (!isGrounded && !isWallrunning && (wallrunCheckLeft || wallrunCheckRight) && wallToRun != null)
        {
            if (wallrunCheckLeft) wallrunSide = "left";
            else if (wallrunCheckRight) wallrunSide = "right";

            var wallrunDirection = wallToRun.transform.forward;
            isWallrunning = true;
            rb.useGravity = false;
            Debug.Log("Started wallrun on side " + wallrunSide);
            wallrunTimer = 0;
            StartCoroutine(wallrunTimerTick());
        }

        // wallrun runtime logic
        if (isWallrunning)
        {
            if ((wallrunSide.Equals("left") && !wallrunCheckLeft) || (wallrunSide.Equals("right") && !wallrunCheckRight) || wallrunTimer >= wallrunTime)
            {
                Debug.Log("Exited wallrun");
                StopCoroutine(wallrunTimerTick());
                isWallrunning = false;
                wallrunCheckLeft = false;
                wallrunCheckRight = false;
                wallToRun = null;
                rb.useGravity = true;
                wallrunSide = "none";
            }
            else {
                float xSpeed = rb.velocity.x;
                float zSpeed = rb.velocity.z;
                if (Mathf.Abs(rb.velocity.magnitude) < maxSpeed)
                {
                    xSpeed += xSpeed * wallAccel;
                    zSpeed += zSpeed * wallAccel;
                }

                rb.velocity = new Vector3(xSpeed, 0, zSpeed);
            }
        }
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        if (!isGrounded) return;
        rb.AddForce(Vector3.up * jumpForce);
    }

    private IEnumerator wallrunTimerTick()
    {
        Debug.Log("Wallrun ticking at " + wallrunTimer);
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        Debug.Log("Wallrun ticking at " + wallrunTimer);
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        Debug.Log("Wallrun ticking at " + wallrunTimer);
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        Debug.Log("Wallrun ticking at " + wallrunTimer);
    }
}
