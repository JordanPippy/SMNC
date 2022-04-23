using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunStatus", menuName = "Statuses/StunStatus", order = 0)]
public class StunStatus : StatusEffect
{
    public override void StartEffect(GameObject tar)
    {
        Movement movementScript = tar.GetComponent<Movement>();
        movementScript.SetMoveSpeedMod(movementScript.GetMoveSpeed() * -1); // Negate the move speed of the target.
    }

    public override void EndEffect(GameObject tar)
    {
        tar.GetComponent<Movement>().SetMoveSpeedMod(0); // Effectively reset the movespeed to normal speed.
    }
}
