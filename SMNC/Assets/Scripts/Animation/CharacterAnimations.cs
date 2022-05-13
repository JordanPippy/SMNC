using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterAnimations : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private Movement movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        //animator.SetBool("isWalking", movement.clientInput.moveDirection != Vector3.zero);
        animator.SetBool("isWalking", movement.isMoving);
    }
}
