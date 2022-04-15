using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBase : ScriptableObject
{
    public virtual void Use(Transform t)
    {
        Debug.Log("Called Use on the base method!");
    }

    public virtual void LoadAssets()
    {
        Debug.Log("This should have been overriden and wasn't");
    }
}
