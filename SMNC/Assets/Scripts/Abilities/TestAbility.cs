using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAbility : AbilityBase
{

    public override void Use() 
    {
        Debug.Log("Called Use on the TestAbility!");
    }
}
