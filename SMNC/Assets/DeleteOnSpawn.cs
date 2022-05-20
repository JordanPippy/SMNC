using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnSpawn : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject);
    }
}
