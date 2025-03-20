using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitController : MonoBehaviour
{
    [Header("Setup")]
    public Transform player; // The transform the camera orbits around
    private Rigidbody rbody;
    private float inputX;
    private float inputY;

    [Header("Movement variables")]
    public float speed = 10;
    public float jumpForce = 5;
    public float torqueSpeed = 20;
    public float damping = 0.9f; // Slowdown factor
    public float maxAngularVelocity = 10f;

    [Header("Camera variables")]
    public float rotationSpeed = 35f; // Speed of camera rotation
    public float zoomSpeed = 2f; // Speed of zoom
    public float minZoom = 1.5f; // Minimum zoom distance
    public float maxZoom = 5f; // Maximum zoom distance
    private float currentZoom; // Current zoom level
    private float targetZoom; // player zoom level for smoothing
    private float currentX = 0f; // Current X (vertical) rotation (for raising/lowering)
    private float currentY = 0f; // Current Y (horizontal) rotation (for orbiting)
    public float minY = 10f; // Minimum Y rotation (clamping)
    public float maxY = 85f; // Maximum Y rotation (clamping)
    private bool isRotating = false; // Flag to track if rotation is active
    private Vector3 lastMousePosition; // To track the mouse movement

    void Start()
    {
        // Set the initial zoom based on the current distance from the player
        currentZoom = Vector3.Distance(transform.position, player.position);
        targetZoom = currentZoom; // Ensure initial targetZoom matches
        rbody = player.GetComponent<Rigidbody>();
    }

    void OnMove(InputValue movementValue)
    {
        if (!this.enabled) { return; }
        Vector2 movementVector = movementValue.Get<Vector2>();
        inputX = movementVector.x;
        inputY = movementVector.y;
    }

    void Update()
    {
        // Capture rotation input
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isRotating)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            currentY += mouseDelta.x * rotationSpeed * Time.deltaTime;
            currentX -= mouseDelta.y * rotationSpeed * Time.deltaTime;
            currentX = Mathf.Clamp(currentX, minY, maxY);
            lastMousePosition = Input.mousePosition;
        }

        // Capture zoom input (but don't apply it directly here)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f) // Avoid micro changes
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    void FixedUpdate()
    {
        // Smoothly interpolate current zoom to player zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.fixedDeltaTime * 5f);

        // Calculate the desired position based on spherical coordinates
        Quaternion rotation = Quaternion.Euler(currentX, currentY, 0);
        Vector3 offset = new Vector3(0f, 0f, -currentZoom); // Use negative zoom for "in" movement
        Vector3 desiredPosition = player.position + rotation * offset;

        // Apply the calculated position and always look at the player
        transform.position = desiredPosition;
        transform.LookAt(player);

        MovePlayer();
    }

    void MovePlayer()
    {
        // Only apply force if there's meaningful input
        if (Mathf.Abs(inputX) > 0.01f || Mathf.Abs(inputY) > 0.01f)
        {
            rbody.AddForce(inputY * transform.forward * speed);
            rbody.AddForce(inputX * transform.right * speed);

            // Apply torque for rolling effect
            Vector3 rotationAxis = Vector3.Cross(rbody.velocity.normalized, GetHorizontalForward(transform));
            rbody.AddTorque(rotationAxis * torqueSpeed, ForceMode.Force);
        }

        // Apply damping, but only if there's movement
        if (rbody.velocity.magnitude > 0.01f)
        {
            rbody.angularVelocity *= damping;
        }

        // Stop movement completely if it's below a small threshold to prevent jitter
        if (rbody.velocity.magnitude < 0.05f)
        {
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
        }

        // Cap the angular velocity to prevent excessive spinning
        if (rbody.angularVelocity.magnitude > maxAngularVelocity)
        {
            rbody.angularVelocity = rbody.angularVelocity.normalized * maxAngularVelocity;
        }
    }

    void OnJump(InputValue jump)
    {
        if (!this.enabled) { return; }
        rbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    Vector3 GetHorizontalForward(Transform transform)
    {
        Quaternion rotation = transform.rotation;
        rotation.x = 0;
        rotation.z = 0;
        rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        return rotation * Vector3.forward;
    }
}