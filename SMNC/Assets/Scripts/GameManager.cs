using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    void Start()
    {
        Database.gm = this;
        Database.LoadAbilityAssets();
    }
    public GameObject testAbilityProjectile;
}
