using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Movement : NetworkBehaviour
{
    public Camera HeadCamera;
    [SerializeField] private float moveSpeed;

    // This may need to become/already is a global variable. REMEMBER GRAVITY IS NEGATIVE PEOPLE. WE. DONT. FLY.
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


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Start()
    {
        move = Vector3.zero;
        jump = false;
        hardFalling = true;
        playerVerticalVelocity.y = gravity * Time.deltaTime;

        HeadCamera.GetComponent<Camera>().enabled = isLocalPlayer;
        HeadCamera.GetComponent<AudioListener>().enabled = isLocalPlayer;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;
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
        if (!isLocalPlayer)
            return;
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

            // Add gravity to player's vertical velocity. 
            //playerVerticalVelocity.y -= gravity;

            /*controller fnc can be split into 2 fncs (move*moveSpeed) and playerVerticalVelocity both 
             *individually multiplied by deltaTime
             */
            controller.Move(((move * moveSpeed)+playerVerticalVelocity) * Time.deltaTime);
            
        }
    }
}