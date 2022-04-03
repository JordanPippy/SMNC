using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float duration;

    void Start()
    {
        duration = 5.0f;
        projectileSpeed = 10.0f;
        if (isServer)
            Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLISION");
        if (isServer)
        {
            //handle bullet collision
            Destroy(gameObject);
        }
        else if (isLocalPlayer)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //move forward, quaternion set in ShootProjectile.cs.
        transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
    }
}
