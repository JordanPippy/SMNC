using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float projectileSpeed = 10.0f;
    [SerializeField] private float duration = 2.0f;
    [SerializeField][SyncVar] public int damage = 20;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
            return;
        Debug.Log("COLLISION with: " + other.gameObject.name);
        if (isServer)
        {
            //handle bullet collision
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);
            //gameObject.SetActive(false);
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

    IEnumerator DestroyInSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(gameObject);
    }
}
