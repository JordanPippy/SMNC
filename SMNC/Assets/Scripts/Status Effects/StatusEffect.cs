using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public string statusName;
    public DuplicateHandling duplicateType = DuplicateHandling.Ignore; // How to handle the same effect being applied multiple times.
    public float tickRate; // How often is During() called in seconds? -1 for disabled
    public virtual void StartEffect(GameObject tar) {} // Called on the first tick.
    public virtual void DuringEffect(GameObject tar) {} // Called after the first and before the last.
    public virtual void EndEffect(GameObject tar) {} // Called when the effect ends.

    // Ignore, do nothing when reapplied. Stack applies more instances to the object. Extends extends the duration if longer than the current one.
    public enum DuplicateHandling{Ignore, Stack, Extend} 
}
