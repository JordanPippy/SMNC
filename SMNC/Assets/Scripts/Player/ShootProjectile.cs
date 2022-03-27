using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShootProjectile : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Projectile projectile;
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private Camera mainCamera;

    private bool shooting;

    void Start()
    {
        shooting = false;
        distanceFromPlayer = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
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

        if (shooting)
        {
            Vector3 projectileSpawnLocation = mainCamera.transform.position + (mainCamera.transform.forward * distanceFromPlayer);
            Instantiate(projectile, projectileSpawnLocation, mainCamera.transform.rotation);
        }
        
    }
}
