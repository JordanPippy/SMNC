using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ShootProjectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject projectile;
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
                RequestProjectileSpawnServerRpc(mainCamera.transform.position, mainCamera.transform.forward, mainCamera.transform.rotation, distanceFromPlayer);
                StartCoroutine(shootingDelay(0.2f));
                //Vector3 projectileSpawnLocation = mainCamera.transform.position + (mainCamera.transform.forward * distanceFromPlayer);
                //Instantiate(projectile, projectileSpawnLocation, mainCamera.transform.rotation);
            }
        }
        
    }
    

    [Command]
    void RequestProjectileSpawnServerRpc(Vector3 pos, Vector3 forward, Quaternion rotation, float distance)
    {
        Vector3 projectileSpawnLocation = pos + (forward * distance);
        GameObject p = Instantiate(projectile, projectileSpawnLocation, rotation);
        NetworkServer.Spawn(p);
        p.GetComponent<MeshRenderer>().enabled = false;
        SpawnProjectileLocallyClientRpc(projectileSpawnLocation, rotation);
    }

    [ClientRpc]
    void SpawnProjectileLocallyClientRpc(Vector3 location, Quaternion rotation)
    {
        Instantiate(projectile, location, rotation);
    }
    

    IEnumerator shootingDelay(float time)
    {
        canShoot = false;
        yield return new WaitForSeconds(time);
        canShoot = true;
    }
}
