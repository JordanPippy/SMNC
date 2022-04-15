using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBase : ScriptableObject
{
    public virtual void Use()
    {
        Debug.Log("Called Use on the base method!");
    }
}
