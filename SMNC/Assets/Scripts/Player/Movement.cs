using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Movement : NetworkBehaviour
{
    public Camera HeadCamera;
    [SyncVar] public bool canMove = true;
    [SyncVar] private float moveSpeed = 5.0f;

    // This may need to become/already is a global variable. REMEMBER GRAVITY IS NEGATIVE PEOPLE. WE. DONT. FLY.
    [SyncVar] private float gravity = -9.81f; 

    // The force of the jump. (Should represent Unity units of height.)
    [SyncVar] private float jumpHeight = 5.0f; 

    // Duration of the jump, shorter means the player reaches max height faster.

    // Used for gravity and jumping.
    private Vector3 playerVerticalVelocity;   

    //floatingJumpModifier is the amount of velocity the player gains by letting go of space during their jump.
    [SyncVar] private float floatingJumpModifier = 1.5f;  
    private CharacterController controller;
    private Player player;

    [SyncVar] private Vector3 networkPosition;
    [SyncVar] public bool isMoving = false;
    public InputData clientInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Start()
    {
        if (isLocalPlayer)
        {
            GetComponent<NetworkTransform>().interpolatePosition = false;
            GetComponent<NetworkTransform>().interpolateRotation = false;
        }
        clientInput = new InputData(Vector3.zero, false, true);

        HeadCamera.GetComponent<Camera>().enabled = isLocalPlayer;
        HeadCamera.GetComponent<AudioListener>().enabled = isLocalPlayer;

        player = GetComponent<Player>();
    }

    void Update()
    {
        GetMovementInput();
        if (isLocalPlayer)
            UpdateIsMoving(clientInput.moveDirection);
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

        MovementCalculation();
        UpdateNetworkPos(clientInput, GetMoveSpeed());
    }

    void GetMovementInput()
    {
        if (!isLocalPlayer || !canMove)
            return;

        // Move the character relative to the direction they are facing.
        clientInput.moveDirection = Input.GetAxisRaw("Horizontal") * transform.right 
                                  + Input.GetAxisRaw("Vertical") * transform.forward;

        // If space is pressed, jump.
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            clientInput.jump = true;
            clientInput.hardFalling = false;
        }

        if (Input.GetButtonUp("Jump"))
            clientInput.hardFalling = true;
    }

    [Command]
    void UpdateNetworkPos(InputData inputs, float clientSpeed)
    {
        float acceptableDifference = 2.0f;
        if (Vector3.Distance(ReportClientPos(), networkPosition) >= acceptableDifference || 
            clientSpeed != GetMoveSpeed())
        {
            Debug.Log("Detected cheating!");
            ForceMoveClient(networkPosition);
        }
        else
            networkPosition = transform.position;
    }

    [Command]
    void UpdateIsMoving(Vector3 movement)
    {
        isMoving = movement != Vector3.zero;
    }

    /*
     * The CharacterController must be disabled before updating the position.
     * Otherwise the fight will be relentless
     */

    [ClientRpc]
    public void ForceMoveClient(Vector3 pos)
    {
        print("Force moved client to " + pos);
        controller.enabled = false;
        transform.position = pos;
        networkPosition = pos;
        StartCoroutine(EnableControllerDelay(0.5f)); // Delay to turn controller back on
    }

    [ClientCallback]
    Vector3 ReportClientPos()
    {
        return transform.position;
    }


    /* 
    * Jumping is handled by applying a constant velocity to the player for
    * a duration of time. This is to prevent the player from snapping to
    * their max jump height, and makes it a bit more smooth. 
    *
    * I THINK I have it so jump height represents the maximum number of Unity 
    * units the player will travel upwards, but don't quote this on that.
    */
    private void CalculateJump()
    {
        if (clientInput.jump)
        {
            clientInput.jump = false;
            playerVerticalVelocity.y = jumpHeight;
        }
        else if (!controller.isGrounded)
        {
            //In this else if is when the player is NOT jumping but is still in the air
            playerVerticalVelocity.y += clientInput.hardFalling ? 
                                        gravity * floatingJumpModifier * Time.deltaTime : // True Case
                                        gravity * Time.deltaTime;                         // False Case
        }
        else 
        {
            //This else is when the player is not jumping and is grounded.
            playerVerticalVelocity.y = gravity * Time.deltaTime;
        }
    }

    void MovementCalculation()
    {
        if (player.currentHealth <= 0 || !controller.enabled || !canMove)
            return;


        CalculateJump();

        clientInput.moveDirection = clientInput.moveDirection.normalized;

        /* controller fnc can be split into 2 fncs (move*moveSpeed) and playerVerticalVelocity both 
         * individually multiplied by deltaTime
         */
        controller.Move(((clientInput.moveDirection * GetMoveSpeed())+playerVerticalVelocity) * Time.deltaTime);   
    }

    IEnumerator EnableControllerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        controller.enabled = true;
    }

    public float GetMoveSpeed()
    {
        return canMove ? moveSpeed : 0;
    }
}

[System.Serializable]
public struct InputData 
{
    public Vector3 moveDirection;
    public bool jump; // Whether the jump key has been pressed.
    public bool hardFalling; //Hard falling is toggle on whether or not the player let go of space during their jump.

    public InputData(Vector3 moveDirection, bool jump, bool hardFalling)
    {
        this.moveDirection = moveDirection;
        this.jump = jump;
        this.hardFalling = hardFalling;
    }
}