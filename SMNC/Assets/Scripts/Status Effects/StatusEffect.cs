using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public string statusName;
    public float tickRate; // How often is During() called?
    public virtual void StartEffect(GameObject tar) {} // Called on the first tick.
    public virtual void DuringEffect(GameObject tar) {} // Called after the first and before the last.
    public virtual void EndEffect(GameObject tar) {} // Called when the effect ends.
}
