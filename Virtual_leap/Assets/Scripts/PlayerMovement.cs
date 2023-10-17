using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public XRNode inputSource;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private GameObject groundCheck;

    private Vector2 inputAxis;
    private XROrigin rig;
    private Rigidbody rb;

    private bool isGrounded => Physics.OverlapSphere(groundCheck.transform.position, .25f, groundLayer).Length > 0;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<XROrigin>();
        rb = GetComponent<Rigidbody>();
        jumpActionReference.action.performed += OnJump;
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out inputAxis);
    }

    void FixedUpdate()
    {
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);

        if (direction.magnitude != 0 && Mathf.Abs(rb.velocity.magnitude) < maxSpeed)
        {
            rb.velocity += direction * acceleration;
        }
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        if (!isGrounded) return;
        rb.AddForce(Vector3.up * jumpForce);
    }
}
