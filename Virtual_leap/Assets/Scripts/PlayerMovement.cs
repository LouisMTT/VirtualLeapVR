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
using Random = UnityEngine.Random;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public XRNode inputSource;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference crouchActionReference;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private GameObject groundCheck;
    [SerializeField] private float wallAccel = 2f;
    [SerializeField] private float wallrunTime = 3f;
    [SerializeField] private float aerialAccel = 0.5f;
    [SerializeField] private float wallrunCooldown = 1f;
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject leftController;
    [SerializeField] private float slideForce = 200f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float slideCooldown = 3f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCamOffset = 0.8f;
    [SerializeField] private CapsuleCollider hitbox;
    [SerializeField] private GameObject cameraOffset;
    [SerializeField] private float slideSpeedThreshold = 5f;

    private Vector2 inputAxis;
    private XROrigin rig;
    private Rigidbody rb;
    public bool wallrunCheckLeft;
    public bool wallrunCheckRight;
    private bool isWallrunning;
    public GameObject wallToRun;
    private float wallrunTimer;
    private String wallrunSide;
    private Coroutine wallrunTick;
    private bool canWallrun;
    private bool isCrouching;
    private float defaultCamOffset;
    private float defaultHitboxHeight;
    private bool canSlide;

    //for jump sounds
    public AudioSource audioSource;
    public AudioClip[] audioSources;

    private bool isGrounded => Physics.OverlapSphere(groundCheck.transform.position, .25f, groundLayer).Length > 0;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<XROrigin>();
        rb = GetComponent<Rigidbody>();
        jumpActionReference.action.performed += OnJump;
        crouchActionReference.action.started += OnCrouch;
        crouchActionReference.action.canceled += OnStand;
        wallrunCheckLeft = false;
        wallrunCheckRight = false;
        isWallrunning = false;
        wallToRun = null;
        wallrunTimer = 0f;
        wallrunSide = "none";
        canWallrun = true;
        defaultCamOffset = cameraOffset.transform.localPosition.y;
        defaultHitboxHeight = hitbox.height;
        canSlide = true;
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
        // head direction and input direction calculations
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
        if (!isGrounded && !isWallrunning && (wallrunCheckLeft || wallrunCheckRight) && wallToRun != null && canWallrun)
        {
            if (wallrunCheckLeft) wallrunSide = "left";
            else if (wallrunCheckRight) wallrunSide = "right";

            var wallrunDirection = wallToRun.transform.forward;
            isWallrunning = true;
            rb.useGravity = false;
            Debug.Log("Started wallrun on side " + wallrunSide);
            wallrunTimer = 0;
            wallrunTick = StartCoroutine(wallrunTimerTick());
        }

        // wallrun runtime logic
        if (isWallrunning)
        {
            // stopping wallrun
            if ((wallrunSide.Equals("left") && !wallrunCheckLeft) || (wallrunSide.Equals("right") && !wallrunCheckRight) || wallrunTimer >= wallrunTime)
            {
                Debug.Log("Exited wallrun");
                StopCoroutine(wallrunTick);
                isWallrunning = false;
                wallrunCheckLeft = false;
                wallrunCheckRight = false;
                wallToRun = null;
                rb.useGravity = true;
                wallrunSide = "none";
            }
            // on-going wallrun
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
        // if player is only in the air, can't jump
        if (!isGrounded && !isWallrunning) return;
        audioSource.clip = audioSources[Random.Range(0, 3)];
        audioSource.Play();

        // if player is not on a wall, normal jump
        if (!isWallrunning) rb.AddForce(Vector3.up * jumpForce);

        // otherwise, wallrun jump
        else
        {
            // first sets the direction to jump to (based on the opposite hand's position) and then applies force upwards and that way
            Vector3 towardsController = new Vector3();
            if (wallrunSide == "left") towardsController = rightController.transform.position - rb.transform.position;
            else if (wallrunSide == "right") towardsController = leftController.transform.position - rb.transform.position;
            else Debug.Log("no wallrun side set but still wallrunning?");
            rb.AddForce(towardsController * jumpForce / 2);
            rb.AddForce(Vector3.up * jumpForce / 2);
            Debug.Log("Jumped off the wall");
        }
    }

    // method that is used to crouch
    private void OnCrouch(InputAction.CallbackContext obj)
    {
        if (!isGrounded) return;

        Debug.Log("Started crouching");
        isCrouching = true;
        hitbox.height = crouchHeight;
        cameraOffset.transform.localPosition = new Vector3(cameraOffset.transform.localPosition.x, -crouchCamOffset, cameraOffset.transform.localPosition.z);

        if (Mathf.Abs(rb.velocity.magnitude) >= slideSpeedThreshold && canSlide)
        {
            Debug.Log("Enough speed to slide");
            canSlide = false;
            Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
            Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
            rb.AddForce(direction * slideForce);
            StartCoroutine(slideCooldownTick(slideCooldown));
        }
    }

    // method that is used to stand up after crouching
    private void OnStand(InputAction.CallbackContext obj)
    {
        Debug.Log("Stopped crouching");
        isCrouching = false;
        hitbox.height = defaultHitboxHeight;
        cameraOffset.transform.localPosition = new Vector3(cameraOffset.transform.localPosition.x, defaultCamOffset, cameraOffset.transform.localPosition.z);
    }

    // used to tick the timer on wallrunning
    private IEnumerator wallrunTimerTick()
    {
        Debug.Log("Started ticking wallrun");
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        yield return new WaitForSecondsRealtime(1);
        wallrunTimer += 1;
        Debug.Log("Ended wallrun ticking");
    }

    private IEnumerator wallrunCooldownTick()
    {
        yield return new WaitForSecondsRealtime(wallrunCooldown);
        canWallrun = true;
        Debug.Log("Can wallrun again");
    }

    private IEnumerator slideCooldownTick(float cooldown)
    {
        yield return new WaitForSecondsRealtime(cooldown);
        canSlide = true;
        Debug.Log("Can slide again");
    }
}
