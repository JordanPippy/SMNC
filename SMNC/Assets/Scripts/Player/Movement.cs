using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    // This may need to become/already is a global variable.
    [SerializeField] private float gravity; 

    // The force of the jump. (Should represent Unity units of height.)
    [SerializeField] private float jumpHeight; 

    // Duration of the jump, shorter means the player reaches max height faster.
    [SerializeField] private float jumpDuration; 

    // Used for gravity and jumping.
    [SerializeField] private Vector3 playerVerticalVelocity;     
    private CharacterController controller;
    private bool jump; // Whether the jump key has been pressed.
    private float jumpDurationLeft;
    private Vector3 move;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        move = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the character relative to the direction they are facing.
        move = Input.GetAxisRaw("Horizontal") * transform.right 
             + Input.GetAxisRaw("Vertical") * transform.forward;

        // If space is pressed, jump.
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            jump = true;
            jumpDurationLeft = jumpDuration;
        }     
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
        playerVerticalVelocity.y = 0f;

        /* 
         * Jumping is handled by applying a constant velocity to the player for
         * a duration of time. This is to prevent the player from snapping to
         * their max jump height, and makes it a bit more smooth. 
         *
         * I THINK I have it so jump height represents the maximum number of Unity 
         * units the player will travel upwards, but don't quote this on that.
         */
        if (jumpDurationLeft > 0)
        {
            playerVerticalVelocity.y += (jumpHeight/jumpDuration) + gravity;
            jumpDurationLeft -= Time.deltaTime;

            if (jumpDurationLeft >= 0)
                jump = false;
        }

        move = move.normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Add gravity to player's vertical velocity. 
        playerVerticalVelocity.y -= gravity; 
        controller.Move(playerVerticalVelocity * Time.deltaTime); 
    }
}
