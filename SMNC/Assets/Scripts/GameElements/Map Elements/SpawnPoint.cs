using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public SpawnTeam spawnAllowed; // For the future.
    public float lastSpawnTime = 0; // Last time spawn was used, also for the future.
    public enum SpawnTeam {Team1, Team2, Anyone} // Controls who can use the spawn.

    public Vector3 GetSpawn()
    {
        lastSpawnTime = Time.time;

        return transform.position;
    }
}
