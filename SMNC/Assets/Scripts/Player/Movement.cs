using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private CharacterController controller;

    private Vector3 move;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveSpeed = 10.0f;
        move = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the character relative to the direction they are facing.
        move = Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward;
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
        move = move.normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
