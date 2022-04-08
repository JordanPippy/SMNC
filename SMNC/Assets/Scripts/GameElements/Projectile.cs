using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float duration;
    [SerializeField][SyncVar] public int damage;

    void Start()
    {
        duration = 2.0f;
        projectileSpeed = 10.0f;
        damage = 20;
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
            return;
        Debug.Log("COLLISION ");
        if (isServer)
        {
            //handle bullet collision
            //Destroy(gameObject);
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
