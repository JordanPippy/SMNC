using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunStatus", menuName = "Statuses/StunStatus", order = 0)]
public class StunStatus : StatusEffect
{
    public override void StartEffect(GameObject tar)
    {
        Movement movementScript = tar.GetComponent<Movement>();
        movementScript.canMove = false;
    }

    public override void EndEffect(GameObject tar)
    {
        tar.GetComponent<Movement>().canMove = true; // Effectively reset the movespeed to normal speed.
    }
}
