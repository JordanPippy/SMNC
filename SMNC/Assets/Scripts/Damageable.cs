using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Damageable : NetworkBehaviour
{
    // Start is called before the first frame update
    Camera mainCamera;
    public HealthBar healthBar;

    public int currentHealth;
    void Start()
    {
        mainCamera = Camera.main;
        gameObject.GetComponent<Canvas>().worldCamera = mainCamera;
        currentHealth = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
            UpdateServer();
    }

    void UpdateServer()
    {
        if (currentHealth == 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        healthBar.changeHealth(-damage);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                int damage = other.gameObject.GetComponent<Projectile>().damage;
                currentHealth -= damage;
                //RpcChangeHealthBar(-damage);
                TakeDamage(damage);
                Destroy(other.gameObject);
            }
        }
    }
}
