using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Throwable", menuName = "Abilities/Throwable", order = 0)]
public class Throwable : AbilityBase
{
    public enum ThrowType {Lob, Straight};

    public ThrowType throwType;
    public float throwStrength;
    public float speed;
    public GameObject thrown;

    public override void Use(Transform t) 
    {
        StartThrow(t);
    }

    public void StartThrow(Transform t)
    {
        switch (throwType)
        {
            case ThrowType.Lob:
                Lob(t);
                break;
            case ThrowType.Straight:
                Straight(t);
                break;
        }
    }

    public void Lob(Transform t)
    {
        Debug.Log(t.rotation);
        //Camera camera = Camera.main;
        //t = camera.transform;
        Vector3 projectileSpawnLocation = t.position + (t.forward * 3.0f);
        GameObject p = Instantiate(thrown, projectileSpawnLocation, t.rotation);
        p.GetComponent<Rigidbody>().AddForce(p.transform.forward * throwStrength, ForceMode.Impulse);
    }

    public void Straight(Transform t)
    {
        //TODO:
        return;
    }
}
