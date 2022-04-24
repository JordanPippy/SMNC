using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Damageable : NetworkBehaviour
{
    // Start is called before the first frame update
    public HealthBar healthBar;

    public int currentHealth, maxHealth;
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isServer)
            UpdateServer();
    }

    void UpdateServer()
    {
        if (currentHealth <= 0)
            OnDeath();
    }

    protected void HealthSetup(int maximum)
    {
        this.maxHealth = maximum;
        this.currentHealth = this.maxHealth;
        this.healthBar.SetMaxHealth(this.maxHealth);
    }

    public abstract void OnDeath();

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        RpcSetHealthBar(currentHealth);
    }

    [ClientRpc]
    public void RpcSetHealthBar(int amount)
    {
        healthBar.SetHealth(amount);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                TakeDamage(other.gameObject.GetComponent<Projectile>().damage);
                Destroy(other.gameObject);
            }
        }
    }
}
