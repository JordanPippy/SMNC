using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectShot", menuName = "Abilities/StatusEffectShot", order = 0)]

public class StatusEffectShot : AbilityBase
{
    public GameObject projectile; // What gameobject to use as the bullet.
    public StatusEffect effect; // The type of effect that will be applied to the target.
    public float duration; // How long this effect will last.

    public override void Use(Transform t) 
    {
        GameObject shot = Instantiate(projectile, t.position + (3.0f * t.forward), t.rotation);

        // Add the component for applying status effects to the projectile.
        ApplyStatusEffect se = shot.AddComponent(typeof(ApplyStatusEffect)) as ApplyStatusEffect;
        shot.GetComponent<ApplyStatusEffect>().OnHitEffect = this;
    }
}
