using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestAbility", menuName = "Abilities/TestAbility", order = 0)]
public class TestAbility : AbilityBase
{
    public GameObject projectile;

    public override void Use(Transform t) 
    {
        if (projectile == null)
        {
            Debug.Log("Yep, TestAbilityProjectile was null");
        }
        Debug.Log("Called Use on: " + base.title);
        Instantiate(projectile, t.position + (3.0f * t.forward), t.rotation);
    }
}
