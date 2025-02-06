using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private float inputX;
    private float inputY;
    public float deathHeight = -10f;
    public float checkRadius = .1f;
    private Vector3 startPosition;
    public bool playerCanJump = true;
    private bool canJump = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //sets start position
        startPosition = transform.position;

    }

    void OnJump(InputValue value)
    {
        //check if we can jump and have the ability to
        if (canJump && playerCanJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }


    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVec = movementValue.Get<Vector2>();
        inputX = movementVec.x;
        inputY = movementVec.y;
    }

    private void FixedUpdate()
    {   //Kill Player if we go to low in the scene
        if (transform.position.y < deathHeight) { PlayerDeath(); }
        {

        }
        //create a Player layer and assign the Player to it
        //This creates a mask to exclude the "Player" layer
        int playerLayerMask = 1 << LayerMask.NameToLayer("Player");

        //Get the current player position and move it down by the radius of the marble
        Vector3 position = transform.position;
        position.y -= GetComponent<SphereCollider>().radius;
        //check if anytthing is touching the bottom of the marble
        //If it is we are touching the ground and can jump
        if (Physics.CheckSphere(position, checkRadius, ~playerLayerMask))
        {
            canJump = true;
        }


        else { canJump = false; }

        //movement
        Vector3 movement = new Vector3(inputX, 0.0f, inputY);
        rb.AddForce(movement * speed);

    }   
    //Kill the Player and restart the level
    public void PlayerDeath()
    {
       transform.position = startPosition;
       rb.angularVelocity = Vector3.zero;
       rb.velocity = Vector3.zero;
    }

    //This creates  visual for us to see where we are checking
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - Vector3.up * GetComponent<SphereCollider>().radius, checkRadius);
    }
}
