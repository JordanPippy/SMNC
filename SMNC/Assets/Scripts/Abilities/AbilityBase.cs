using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : ScriptableObject
{
    public string title;

    public abstract void Use(Transform t);
}
