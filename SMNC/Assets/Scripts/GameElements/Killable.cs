using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killable : Damageable
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        base.HealthSetup(maxHealth);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void OnDeath()
    {
        Destroy(gameObject);
    }
}
