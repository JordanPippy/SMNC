using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    private Transform localPlayer;
    void Start()
    {
        localPlayer = GameObject.Find("LocalPlayer").transform;
    }
    void Update()
    {
        transform.LookAt(localPlayer.transform.position);
    }
}
