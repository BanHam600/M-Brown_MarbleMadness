using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Setup")]
    public Transform cameraRig;  // Camera Rig is the parent that rotates and moves
    public Transform cameraTransform;  // The actual Camera
    private Rigidbody rbody;
    private float inputX;
    private float inputY;

    [Header("Movement Variables")]
    public float speed = 10;
    public float jumpForce = 5;
    public float torqueSpeed = 20;
    public float turnSpeed = 120;
    public float damping = 0.9f; // Slowdown factor
    public float maxAngularVelocity = 10f;

    [Header("Camera Variables")]
    public float distance = 5f; // Default camera distance from the target
    public float minDistance = 2f; // Minimum camera distance to the marble
    public float smoothSpeed = 0.05f; // Adjusted for quicker camera zoom
    public float collisionOffset = 0.5f; // How much closer the camera should get if an obstacle is detected
    public LayerMask collisionLayer; // Layer mask for walls and obstacles

    // Camera height offset
    public float heightOffset = 2f; // Offset for camera height above the cameraRig

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 previousCameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        previousCameraPosition = cameraTransform.position;
    }

    void OnMove(InputValue movementValue)
    {
        if (!this.enabled) { return; }
        Vector2 movementVector = movementValue.Get<Vector2>();
        inputX = movementVector.x;
        inputY = movementVector.y;
    }

    void OnJump(InputValue jump)
    {
        if (!this.enabled) { return; }
        rbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Rotate the player based on input
        RotatePlayer();

        // Move the player based on input
        MovePlayer();

        // Update cameraRig position to follow the player
        FollowPlayer();

        // Handle camera collision and adjust camera position
        HandleCameraCollision();
    }

    // Rotate the player based on input
    void RotatePlayer()
    {
        // Rotate cameraRig based on input
        cameraRig.Rotate(0, inputX * turnSpeed * Time.fixedDeltaTime, 0);
    }

    // Move the player based on input
    void MovePlayer()
    {
        // Only apply force if input is significant
        if (Mathf.Abs(inputY) > 0.01f)
        {
            rbody.AddForce(inputY * cameraRig.forward * speed);
        }

        if (Mathf.Abs(inputX) > 0.01f)
        {
            Vector3 rotationAxis = Vector3.Cross(rbody.velocity.normalized, cameraRig.forward);
            rbody.AddTorque(rotationAxis * torqueSpeed, ForceMode.Force);
        }

        // Apply damping only when moving
        if (rbody.velocity.magnitude > 0.05f)
        {
            rbody.angularVelocity *= damping;
        }
        else
        {
            // Hard stop if below a small threshold
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
        }

        // Cap the angular velocity
        if (rbody.angularVelocity.magnitude > maxAngularVelocity)
        {
            rbody.angularVelocity = rbody.angularVelocity.normalized * maxAngularVelocity;
        }
    }

    // Make the cameraRig follow the player
    void FollowPlayer()
    {
        // Follow the player's position with cameraRig
        cameraRig.position = transform.position;
    }

    // Camera Collision Logic
    void HandleCameraCollision()
    {
        // Calculate the desired camera position based on the cameraRig's position and the height offset
        Vector3 desiredCameraPosition = cameraRig.position - cameraRig.forward * distance + Vector3.up * heightOffset;

        // Raycast to detect obstacles between the camera and the cameraRig
        RaycastHit hit;
        if (Physics.Raycast(cameraRig.position, -cameraRig.forward, out hit, distance, collisionLayer))
        {
            // If an obstacle is detected, adjust the camera's position closer to the cameraRig
            desiredCameraPosition = hit.point + cameraRig.forward * collisionOffset + Vector3.up * heightOffset;
        }

        // Ensure the camera doesn't get too close to the player
        float currentDistance = Vector3.Distance(cameraTransform.position, transform.position);
        if (currentDistance < minDistance)
        {
            desiredCameraPosition = transform.position - cameraRig.forward * minDistance + Vector3.up * heightOffset;
        }

        // Smoothly move the camera to the adjusted position with stability
        if (Vector3.Distance(previousCameraPosition, desiredCameraPosition) > 0.01f)
        {
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredCameraPosition, ref currentVelocity, smoothSpeed);
        }
        else
        {
            cameraTransform.position = desiredCameraPosition;
        }

        // Update the previous camera position for comparison
        previousCameraPosition = cameraTransform.position;

        // Always make the camera look at the cameraRig (target)
        cameraTransform.LookAt(cameraRig.position);
    }
}