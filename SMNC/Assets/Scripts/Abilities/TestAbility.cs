using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAbility : AbilityBase
{
    private GameObject projectile;

    public override void Use(Transform t) 
    {
        if (projectile == null)
        {
            Debug.Log("Yep, TestAbilityProjectile was null");
            projectile = Database.gm.testAbilityProjectile;
        }
        Debug.Log("Called Use on the TestAbility!");
        Instantiate(projectile, t.position + (3.0f * t.forward), t.rotation);
    }

    public override void LoadAssets()
    {
        projectile = Database.gm.testAbilityProjectile;
    }
}
