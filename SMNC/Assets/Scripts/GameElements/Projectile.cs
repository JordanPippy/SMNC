using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float duration;

    void Start()
    {
        duration = 5.0f;
        projectileSpeed = 10.0f;
        Destroy(gameObject, duration);
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
