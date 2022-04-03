using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShootProjectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Projectile projectile;
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private Camera mainCamera;

    private bool canShoot;

    private bool shooting;

    void Start()
    {
        canShoot = true;
        shooting = false;
        distanceFromPlayer = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            // If Mouse 1 is down (not ON down, while down.) shooting = true.
            shooting = Input.GetButton("Fire1") ? true : false;
        }
    }

    void FixedUpdate()
    {
        /*
         * Make the location in front of the player and about halfway up the player.
         * Thats where the projectile will spawn.
         *
         * Uses the projectile class, which allows for custom projectiles.
         *
         *
         */

        if (shooting && canShoot)
        {
            if (isLocalPlayer)
            {
                //RequestProjectileSpawnServerRpc(mainCamera.transform.position, mainCamera.transform.forward, mainCamera.transform.rotation, distanceFromPlayer);
                StartCoroutine(shootingDelay(0.2f));
                //Vector3 projectileSpawnLocation = mainCamera.transform.position + (mainCamera.transform.forward * distanceFromPlayer);
                //Instantiate(projectile, projectileSpawnLocation, mainCamera.transform.rotation);
            }
        }
        
    }
    /*

    [ServerRpc]
    public void RequestProjectileSpawnServerRpc(Vector3 pos, Vector3 forward, Quaternion rotation, float distance)
    {
        Vector3 projectileSpawnLocation = pos + (forward * distance);
        Projectile p = Instantiate(projectile, projectileSpawnLocation, rotation);
        p.gameObject.GetComponent<NetworkObject>().Spawn();
    }
    */

    IEnumerator shootingDelay(float time)
    {
        canShoot = false;
        yield return new WaitForSeconds(time);
        canShoot = true;
    }
}
