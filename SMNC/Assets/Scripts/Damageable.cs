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
        {
            Destroy(gameObject);
            OnDeath();
        }
    }

    public abstract void OnDeath();

    public void TakeDamage(int damage)
    {
        healthBar.changeHealth(-damage);
        currentHealth -= damage;
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
