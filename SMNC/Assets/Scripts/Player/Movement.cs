using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Movement : NetworkBehaviour
{
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    public Camera HeadCamera;
    [SerializeField] private float moveSpeed;

    // This may need to become/already is a global variable. REMEMBER GRAIVITY IS NEGATIVE PEOPLE. WE. DONT. FLY.
    [SerializeField] private float gravity; 

    // The force of the jump. (Should represent Unity units of height.)
    [SerializeField] private float jumpHeight; 

    // Duration of the jump, shorter means the player reaches max height faster.

    // Used for gravity and jumping.
    private Vector3 playerVerticalVelocity;   

    //floatingJumpModifier is the amount of velocity the player gains by letting go of space during their jump.
    [SerializeField] private float floatingJumpModifier;  
    private CharacterController controller;
    private bool jump; // Whether the jump key has been pressed.

    private Vector3 move;


    //Hard falling is toggle on whether or not the player let go of space during their jump.
    private bool hardFalling;
    public float horizontal, vertical;
    private Player player;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
            networkPosition.Value = new Vector3(5f, 6f, 5f);
    }
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Start()
    {
        if (IsClient && IsOwner)
        {
            move = Vector3.zero;
            jump = false;
            hardFalling = true;
            playerVerticalVelocity.y = gravity * Time.deltaTime;
        }

        HeadCamera.GetComponent<Camera>().enabled = IsLocalPlayer;
        HeadCamera.GetComponent<AudioListener>().enabled = IsLocalPlayer;
    }

    void Update()
    {
        // Move the character relative to the direction they are facing.
        move = Input.GetAxisRaw("Horizontal") * transform.right 
            + Input.GetAxisRaw("Vertical") * transform.forward;

        // If space is pressed, jump.
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            jump = true;
            hardFalling = false;
        }

        if (Input.GetButtonUp("Jump"))
            hardFalling = true;
    }

    [ServerRpc]
    public void RequestMovementServerRpc(Vector3 pos)
    {
        networkPosition.Value = pos;
    }

    /*
    * FixedUpdate is where movement/physics based operations should take place.
    * Do not try to do them in Update().
    *
    * move = move.normalized will normalize the movement vector.
    * This means that the character will move the same speed 
    * diagonally as the do straight. Because pythagoras.
    *
    */
    
    void FixedUpdate()
    {
        if (!IsLocalPlayer)
            transform.position = networkPosition.Value;
        else
        {
            /* 
            * Jumping is handled by applying a constant velocity to the player for
            * a duration of time. This is to prevent the player from snapping to
            * their max jump height, and makes it a bit more smooth. 
            *
            * I THINK I have it so jump height represents the maximum number of Unity 
            * units the player will travel upwards, but don't quote this on that.
            */

            //if the player is jumping (duh)
            if (jump)
            {
                jump = false;
                playerVerticalVelocity.y = jumpHeight;
            }
            else if (!controller.isGrounded)
            {
                //In this else if is when the player is NOT jumping but is still in the air
                if (hardFalling)
                    playerVerticalVelocity.y += (gravity * floatingJumpModifier) * Time.deltaTime;
                else if (Input.GetButton("Jump"))
                    playerVerticalVelocity.y += gravity * Time.deltaTime;    
            }
            else 
            {
                //This else is when the player is not jumping and is grounded.
                playerVerticalVelocity.y = gravity * Time.deltaTime;
            }
        

            move = move.normalized;
            controller.Move(move * moveSpeed * Time.deltaTime);

            // Add gravity to player's vertical velocity. 
            //playerVerticalVelocity.y -= gravity;
            controller.Move(playerVerticalVelocity * Time.deltaTime); 

            RequestMovementServerRpc(transform.position);
        }
    }
}