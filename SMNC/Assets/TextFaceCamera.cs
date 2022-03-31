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
        Vector3 newRotation = localPlayer.transform.position; // To flip the text towards the camera.
        transform.LookAt(newRotation);
    }
}
