using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Initialize a singleton for the spawn manager
    public static SpawnManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public SpawnLogic spawnLogic;
    void Start()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i).GetComponent<SpawnPoint>());
        }
    }

    public Vector3 GetAvailableSpawnPoint()
    {
        switch(spawnLogic)
        {
            case SpawnLogic.Random:
                return spawnPoints[Random.Range(0,spawnPoints.Count)].GetSpawn();
            
            case SpawnLogic.RoundRobin:
                SpawnPoint sp = spawnPoints[0]; // Basically doing a FIFO operation, taking the first element and moving it to the last in the list.
                spawnPoints.RemoveAt(0);
                spawnPoints.Add(sp);
                return sp.GetSpawn();
        }
        return Vector3.zero; // We better not reach here or it means I goofed. 
    }

    public enum SpawnLogic {Random, RoundRobin}
    
}
